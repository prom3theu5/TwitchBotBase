﻿using System;
using System.Collections.Generic;

namespace PromBot.CommandModules.Dice.Models
{
    public class DiceBag
    {
        public enum Dice : uint
        {
            /// <summary>
            /// This can be considered a double-sided coin;
            /// used to delimit a 50/50 probability.
            /// </summary>
            D2 = 2,
            /// <summary>
            /// A Tetrahedron
            /// A 4 Sided Die
            /// </summary>
            D4 = 4,
            /// <summary>
            /// A Cube
            /// A 6 Sided Die
            /// </summary>
            D6 = 6,
            /// <summary>
            /// A Octahedron
            /// A 8 Sided Die
            /// </summary>
            D8 = 8,
            /// <summary>
            /// A Pentagonal Trapezohedron
            /// A 10 Sided Die
            /// </summary>
            D10 = 10,
            /// <summary>
            /// A Dodecahedron
            /// A 12 Sided Die
            /// </summary>
            D12 = 12,
            /// <summary>
            /// A Icosahedron
            /// A 20 Sided Die
            /// </summary>
            D20 = 20,
            /// <summary>
            /// A Rhombic Triacontahedron
            /// A 30 Sided Die
            /// </summary>
            D30 = 30,
            /// <summary>
            /// A Icosakaipentagonal Trapezohedron
            /// A 50 Sided Die
            /// </summary>
            D50 = 50,
            /// <summary>
            /// A Pentagonal Hexecontahedron
            /// A 60 Sided Die
            /// </summary>
            D60 = 60,
            /// <summary>
            /// A Zocchihedron
            /// A 100 Sided Die
            /// </summary>
            D100 = 100
        };

        private Random _rng;

        public DiceBag()
        {
            _rng = new Random();
        }

        /**
         * The default dice-rolling method. All methods link to this one.
         */
        private int InternalRoll(uint dice)
        {
            return 1 + _rng.Next((int)dice);
        }

        /// <summary>
        /// Rolls the specified dice.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>The Number rolled.</returns>
        public int Roll(Dice d)
        {
            return InternalRoll((uint)d);
        }

        /// <summary>
        /// Rolls the chosen dice then adds a modifier
        /// to the rolled number.
        /// </summary>
        /// <param name="dice">The dice.</param>
        /// <param name="modifier">The modifier.</param>
        /// <returns></returns>
        public int RollWithModifier(Dice dice, uint modifier)
        {
            return InternalRoll((uint)dice) + (int)modifier;
        }

        /// <summary>
        /// Rolls a series of dice and returns a collection containing them.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="times">The times.</param>
        /// <returns>A Collection Holding the dice rolls.</returns>
        public List<int> RollQuantity(Dice d, uint times)
        {
            List<int> rolls = new List<int>();
            for (int i = 0; i < times; i++)
            {
                rolls.Add(InternalRoll((uint)d));
            }
            return rolls;
        }
    }
}