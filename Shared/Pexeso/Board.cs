using System;
using System.Collections.Generic;

namespace POT.Pexeso.Shared.Pexeso
{
    public class Board
    {
        public readonly int _width;
        public readonly int _height;

        private Card[,] _board;
        public List<Card> Cards { get; private set; }

        public Board(BoardSize size, List<Card> cards = null)
        {
            GameSettings.ParseWidhtAndHeight(size, out _width, out _height);

            InitGrid(cards);
        }

        private void InitGrid(List<Card> cards)
        {
            _board = new Card[_height, _width];

            if (cards == null) {
                var pairsNeeded = _width * _height / 2;
                cards = new List<Card>(pairsNeeded * 2);

                // create cards
                for (int i = 0; i < pairsNeeded; i++) {
                    cards.Add(new Card { Data = CardSymbols.Symbols[i] });
                    cards.Add(new Card { Data = CardSymbols.Symbols[i] });
                }

                // shuffle cards
                var rnd = new Random();
                var count = cards.Count;
                while (count > 1) {
                    count--;
                    var randomIndex = rnd.Next(count + 1);
                    var value = cards[randomIndex];
                    cards[randomIndex] = cards[count];
                    cards[count] = value;
                }
            }

            Cards = cards;
            // assign cards
            for (int i = 0; i < _height; i++) {
                for (int j = 0; j < _width; j++) {
                    _board[i, j] = cards[i * _width + j];
                }
            }
        }

        public Card this[int height, int width] {
            get => _board[height, width];
        }

        public void Flip(int height, int width)
        {
            _board[height, width].IsFlipped = !_board[height, width].IsFlipped;
        }
    }
}
