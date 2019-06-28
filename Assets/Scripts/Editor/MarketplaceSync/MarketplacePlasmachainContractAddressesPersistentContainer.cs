using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class MarketplacePlasmachainContractAddressesPersistentContainer : ScriptableObject
    {
        private const string AssetPath = "Assets/Resources/Editor/" + nameof(MarketplacePlasmachainContractAddressesPersistentContainer) + ".asset";

        private static MarketplacePlasmachainContractAddressesPersistentContainer _instance;

        public static MarketplacePlasmachainContractAddressesPersistentContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<MarketplacePlasmachainContractAddressesPersistentContainer>($"Editor/{nameof(MarketplacePlasmachainContractAddressesPersistentContainer)}");
                }

                return _instance;
            }
        }

        public static void CreateInstance()
        {
            if (Instance != null)
                throw new InvalidOperationException("Instance already exists");

            _instance = ScriptableObject.CreateInstance<MarketplacePlasmachainContractAddressesPersistentContainer>();
            AssetDatabase.CreateAsset(_instance, AssetPath);
            AssetDatabase.Refresh();
        }

        [SerializeField]
        private MarketplacePlasmachainContractAddressesNetworks _contractAddressesNetworks;

        public MarketplacePlasmachainContractAddressesNetworks ContractAddressesNetworks
        {
            get => _contractAddressesNetworks;
            set => _contractAddressesNetworks = value;
        }
    }
}
