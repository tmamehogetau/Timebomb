using System.Collections.Generic;
using UnityEngine;

namespace Rounds2.Cards
{
    [DisallowMultipleComponent]
    public sealed class PlayerCardLoadout : MonoBehaviour
    {
        private readonly PlayerCardCollection cards = new();

        public int CardCount => cards.Count;
        public IReadOnlyList<CardId> Cards => cards.Cards;
        public CombatCardStats Stats => cards.BuildStats();

        public void Grant(CardId card)
        {
            cards.Grant(card);
        }

        public void Clear()
        {
            cards.Clear();
        }
    }
}
