using System;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class MarketplaceSyncWindow : EditorWindow
    {
        [MenuItem("Window/ZombieBattleground/Open Marketplace Data Sync Window")]
        private static void ShowWindow()
        {
            MarketplaceSyncWindow window = GetWindow<MarketplaceSyncWindow>();
            window.titleContent = new GUIContent("Marketplace Data Sync");
        }

        [SerializeField]
        private MarketplaceCardLibrarySyncGui _marketplaceCardLibrarySyncGui;

        [SerializeField]
        private MarketplacePlasmachainContractAddressCheckGui _marketplacePlasmachainContractAddressCheckGui;

        private Vector2 _scrollPosition;

        private Tab _currentTab;

        private void Awake()
        {
            _marketplaceCardLibrarySyncGui = new MarketplaceCardLibrarySyncGui(this);
            _marketplacePlasmachainContractAddressCheckGui = new MarketplacePlasmachainContractAddressCheckGui(this);
        }

        private void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                DrawMainGui();
            }
        }

        private void DrawMainGui()
        {
            _currentTab = (Tab) GUILayout.Toolbar(
                (int) _currentTab,
                new []
                {
                    "Card Library Sync",
                    "Contract Address Check"
                },
                "LargeButton"
            );
            switch (_currentTab)
            {
                case Tab.CardLibrarySync:
                    _marketplaceCardLibrarySyncGui.Draw();
                    break;
                case Tab.PlasmachainContractAddressesSync:
                    _marketplacePlasmachainContractAddressCheckGui.Draw();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum Tab
        {
            CardLibrarySync,
            PlasmachainContractAddressesSync
        }
    }
}
