using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.Instrumentation;
using WorkspaceServer.Transformations;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public static class InstrumentationLineMapper
    {
        public static async System.Threading.Tasks.Task<(AugmentationMap, VariableLocationMap)> MapLineLocationsRelativeToViewportAsync(
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
                var augmentations = augmentationMap.Data.Values.Select(augmentation =>
                {
                    var filePosition = augmentation.CurrentFilePosition;
                    var filePositionLine = filePosition.Line;

                    if (withinViewport(filePositionLine, viewportSpan))
                    {
                        var newLinePosition = calculateOffset(filePositionLine, viewportSpan);

                        return augmentation.withPosition(
                            new FilePosition
                            {
                                Line = newLinePosition,
                                Character = filePosition.Character,
                                File = filePosition.File
                            }
                        );
                    }
                    else return null;
                }
                ).Where(x => x != null);

                return new AugmentationMap(augmentations.ToArray());
            }

            VariableLocationMap OffsetVariableLocations()
            {
                var variableLocationDictionary = locations.Data.ToDictionary(
                   kv => kv.Key,
                   kv =>
                   {
                       HashSet<VariableLocation> variableLocations = kv.Value;
                       return variableLocations.Select(location =>
                       {
                           var startLine = location.StartLine;
                           var endLine = location.EndLine;

                           if (withinViewport(startLine, viewportSpan) && withinViewport(endLine, viewportSpan))
                           {
                               int newStartLine = (int)calculateOffset(startLine, viewportSpan);
                               int newEndLine = (int)calculateOffset(endLine, viewportSpan);

                               return new VariableLocation(
                                   location.Variable,
                                   newStartLine,
                                   newEndLine,
                                   location.StartColumn,
                                   location.EndColumn
                               );
                           }
                           else return null;
                       })
                           .Where(x => x != null)
                           .ToHashSet();
                   }
                );

                return new VariableLocationMap
                {
                    Data = variableLocationDictionary
                };
            }
        }

        private static bool withinViewport(long line, LinePositionSpan viewportSpan) =>
            line < viewportSpan.End.Line && line > viewportSpan.Start.Line;

        private static long calculateOffset(long line, LinePositionSpan viewportSpan)
        {
            var firstLineInViewport = viewportSpan.Start.Line + 1;
            return line - firstLineInViewport;
        }

        public static IEnumerable<Viewport> FilterActiveViewport(IEnumerable<Viewport> viewports, string activeBufferId)
        {
            var activeFile = activeBufferId.Split("@").First();
            return viewports.Where(viewport => viewport.Destination.Name == activeFile && viewport.Name == activeBufferId);
        }
    }
}

