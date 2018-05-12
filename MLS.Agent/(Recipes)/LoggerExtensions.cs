using System;
using System.Runtime.CompilerServices;
using Pocket;

namespace Recipes
{
    internal static class LoggerExtensions
    {
        internal static ConfirmationLogger OnEnterAndConfirmOnExit(
            this Logger logger,
            string name,
            object[] properties) =>
            new ConfirmationLogger(
                name,
                logger.Category,
                null,
                null,
                true,
                properties);
    }
}
