using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.SH
{
    enum Card
    {
        Red = 'R',
        Blue = 'B'
    }
    class Deck
    {
        StringBuilder deckRB;

        public Deck(int fascCount, int libCount)
        {
            var str = new char[fascCount + libCount];
            for (int i = 0; i < fascCount; i++)
            {
                str[i] = (char)Card.Red;
            }
            for (int i = fascCount; i < fascCount + libCount; i++)
            {
                str[i] = (char)Card.Blue;
            }
            DamnMethods.Shuffle(str);
            deckRB = new StringBuilder(str.ToString());
        }
        public Deck() : this(11, 6) { }

        public Card[] ShowThreeCards()
        {
            var ans = new Card[3];
            for (int i = 0; i < 3; i++)
            {
                ans[i] = (Card)deckRB[i];
            }
            return ans;
        }
        public Card[] DropThreeCards()
        {
            var ans = ShowThreeCards();
            deckRB.Remove(0, 3);
            return ans;
        }
        public Card DropOneCard()
        {
            Card ans = (Card)deckRB[0];
            deckRB.Remove(0, 1);
            return ans;
        }
        public void PushOneCard(Card what)
        {
            deckRB.AppendFormat(((char)what).ToString());
        }
    }
}
