using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEditor;

namespace Loom.ZombieBattleground.Editor.CardLibraryEditor
{
    [Serializable]
    internal class CardVariantGuiItem : CardGuiItem
    {
        private Card _standardCard;

        public Card StandardCard
        {
            get => _standardCard;
            set
            {
                if (_standardCard == value)
                    return;

                _standardCard = value;
                UpdatePreviewCard();
            }
        }

        public CardVariantGuiItem(EditorWindow owner, Card variant, Card standardCard) : base(owner)
        {
            Card = new NullAbilitiesCardProxy(new Card(
                variant.CardKey,
                variant.Set,
                variant.Name,
                variant.Cost,
                variant.Description,
                variant.FlavorText,
                variant.Picture,
                variant.Damage,
                variant.Defense,
                variant.Faction,
                variant.Frame,
                variant.Kind,
                variant.Rank,
                variant.Type,
                variant.Abilities,
                variant.PictureTransforms,
                variant.UniqueAnimation,
                variant.Hidden,
                variant.Overrides
            ));
            _previewUtility = new PreviewRenderUtility();
            StandardCard = standardCard;
            UpdatePreviewCard();
        }

        protected override bool DrawCardEditorGui()
        {
            EditorGUILayout.HelpBox("Editing card variants not implemented, edit the Standard variant", MessageType.Warning);
            return false;
        }

        protected override void UpdatePreviewCard()
        {
            if (StandardCard == null)
                return;

            Card variantCard = DataUtilities.ApplyCardVariantOverrides(Card, StandardCard);
            PreviewCard = new Card(
                variantCard.CardKey,
                variantCard.Set,
                variantCard.Name,
                variantCard.Cost,
                variantCard.Description,
                variantCard.FlavorText,
                variantCard.Picture,
                variantCard.Damage,
                variantCard.Defense,
                variantCard.Faction,
                variantCard.Frame,
                variantCard.Kind,
                variantCard.Rank,
                variantCard.Type,
                variantCard.Abilities,
                variantCard.PictureTransforms,
                // Disable animation for preview
                Enumerators.UniqueAnimation.None,
                variantCard.Hidden,
                variantCard.Overrides
            );

            Title = PreviewCard.ToString();
            ClearPreview();
        }

        private class NullAbilitiesCardProxy : IReadOnlyCard
        {
            private readonly IReadOnlyCard _original;

            public NullAbilitiesCardProxy(IReadOnlyCard original)
            {
                _original = original;
            }

            public CardKey CardKey => _original.CardKey;

            public Enumerators.CardSet Set => _original.Set;

            public string Name => _original.Name;

            public int Cost => _original.Cost;

            public string Description => _original.Description;

            public string FlavorText => _original.FlavorText;

            public string Picture => _original.Picture;

            public int Damage => _original.Damage;

            public int Defense => _original.Defense;

            public Enumerators.Faction Faction => _original.Faction;

            public string Frame => _original.Frame;

            public Enumerators.CardKind Kind => _original.Kind;

            public Enumerators.CardRank Rank => _original.Rank;

            public Enumerators.CardType Type => _original.Type;

            // Returns null since abilities are ignored for variants anyway, but show up in JSON
            public IReadOnlyList<AbilityData> Abilities => null;

            public CardPictureTransforms PictureTransforms => _original.PictureTransforms;

            public Enumerators.UniqueAnimation UniqueAnimation => _original.UniqueAnimation;

            public bool Hidden => _original.Hidden;

            public CardOverrideData Overrides => _original.Overrides;
        }
    }
}