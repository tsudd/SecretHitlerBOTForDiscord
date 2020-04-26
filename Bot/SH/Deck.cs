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
        private StringBuilder deckRB;
        private StringBuilder discard;

        public string DeckRB { get => deckRB.ToString(); }
        public string Discard { get => discard.ToString(); }

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
            deckRB = new StringBuilder(new string(str));
            discard = new StringBuilder();
        }
        public Deck() : this(11, 6) { }

        private void RefillDeck()
        {
            var str = discard.ToString().ToCharArray();
            DamnMethods.Shuffle(str);
            deckRB.Append(str);
            discard.Clear();
        }
        public Card[] ShowThreeCards()
        {
            if (deckRB.Length < 3)
            {
                RefillDeck();
            }
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
            if (deckRB.Length < 1)
            {
                RefillDeck();
            }
            Card ans = (Card)deckRB[0];
            deckRB.Remove(0, 1);
            return ans;
        }
        public void PushOneCard(Card what)
        {
            discard.Append((char)what);
        }
    }
}
