using System;
using System.Collections.Generic;
using System.Text;

namespace Bot
{
    enum Options
    {
        Token,
        Language
    }

    class BotConfig
    {
        public string Token { get; set; }

        public BotConfig()
        {

        }
    }
}
