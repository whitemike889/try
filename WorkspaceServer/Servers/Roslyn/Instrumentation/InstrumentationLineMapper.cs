using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.Instrumentation;
using WorkspaceServer.Transformations;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public static class InstrumentationLineMapper
    {
        public static async Task<(AugmentationMap, VariableLocationMap)> MapLineLocationsRelativeToViewportAsync(
            AugmentationMap augmentationMap,
            VariableLocationMap locations,
            Document document,
            Viewport viewport = null)
        {
            if (viewport == null)
            {
                return (augmentationMap, locations);
            }

            var text = await document.GetTextAsync();
            var viewportSpan = TextSpanToLinePositionSpanTransformer.Convert(viewport.Region, text);

            var mappedAugmentations = OffsetAugmentationFilePositions();
            var mappedLocations = OffsetVariableLocations();

            return (mappedAugmentations, mappedLocations);



            AugmentationMap OffsetAugmentationFilePositions()
            {
                var augmentations = augmentationMap.Data.Values
                    .Where(augmentation => WithinViewport(augmentation.CurrentFilePosition.Line, viewportSpan))
                    .Select(augmentation => MapAugmentationToViewport(augmentation, viewportSpan));

                return new AugmentationMap(augmentations.ToArray());
            }

            VariableLocationMap OffsetVariableLocations()
            {
                var variableLocationDictionary = locations.Data.ToDictionary(
                   kv => kv.Key,
                   kv =>
                   {
                       HashSet<VariableLocation> variableLocations = kv.Value;
                       return variableLocations
                           .Where(loc => WithinViewport(loc.StartLine, viewportSpan) && WithinViewport(loc.EndLine, viewportSpan))
                           .Select(location => MapVariableLocationToViewport(location, viewportSpan))
                           .ToHashSet();
                   }
                );

                return new VariableLocationMap
                {
                    Data = variableLocationDictionary
                };
            }
        }

        private static bool WithinViewport(long line, LinePositionSpan viewportSpan) =>
            line < viewportSpan.End.Line && line > viewportSpan.Start.Line;

        private static long CalculateOffset(long line, LinePositionSpan viewportSpan)
        {
            var firstLineInViewport = viewportSpan.Start.Line + 1;
            return line - firstLineInViewport;
        }

        private static Augmentation MapAugmentationToViewport(Augmentation input, LinePositionSpan viewportSpan) => input.withPosition(
            new FilePosition
            {
                Line = CalculateOffset(input.CurrentFilePosition.Line, viewportSpan),
                Character = input.CurrentFilePosition.Character,
                File = input.CurrentFilePosition.File
            }
        );

        private static VariableLocation MapVariableLocationToViewport(VariableLocation input,
            LinePositionSpan viewportSpan) => new VariableLocation(
                input.Variable,
                (int)CalculateOffset(input.StartLine, viewportSpan),
                (int)CalculateOffset(input.EndLine, viewportSpan),
                input.StartColumn,
                input.EndColumn
            );

        public static IEnumerable<Viewport> FilterActiveViewport(IEnumerable<Viewport> viewports, string activeBufferId)
        {
            var activeFile = activeBufferId.Split("@").First();
            return viewports.Where(viewport => viewport.Destination.Name == activeFile && viewport.Name == activeBufferId);
        }
    }
}

