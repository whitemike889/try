using System;

namespace BasicConsole
{
    class Cat
    {
        public string Say() 
        {
            #region WhatToSay
            var text = "meow! meow!";
            return text[^5..^0];
            #endregion
        }
    }
}
