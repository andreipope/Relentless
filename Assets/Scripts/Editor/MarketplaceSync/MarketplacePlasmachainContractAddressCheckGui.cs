using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using EditorUtility = UnityEditor.EditorUtility;

namespace Loom.ZombieBattleground
{
    [Serializable]
    public class MarketplacePlasmachainContractAddressCheckGui
    {
        private const string BattlegroundMarketplaceSrcContractsPathPrefsKey =
            "MarketplaceContractAddressCheck_BattlegroundMarketplaceSrcContractsPathPrefs";

        [SerializeField]
        private EditorWindow _ownerWindow;

        private string _battlegroundMarketplaceSrcContractsPath;

        [NonSerialized]
        private MarketplacePlasmachainContractAddressesNetworks _contractAddressesNetworks;

        public MarketplacePlasmachainContractAddressCheckGui() { }

        public MarketplacePlasmachainContractAddressCheckGui(EditorWindow ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }

        public void Draw()
        {
            _battlegroundMarketplaceSrcContractsPath =
                EditorSpecialGuiUtility.DrawPersistentFolderPathField(
                    _battlegroundMarketplaceSrcContractsPath,
                    @"Contracts Root ('battleground-marketplace\src\contracts\') Folder Path: ",
                    @"Select CardFaucet 'battleground-marketplace\src\contracts\' Folder",
                    "contracts",
                    BattlegroundMarketplaceSrcContractsPathPrefsKey,
                    430
                );

            if (!Directory.Exists(_battlegroundMarketplaceSrcContractsPath))
            {
                GUILayout.Label("Select a valid folder.");
                return;
            }

            if (MarketplacePlasmachainContractAddressesPersistentContainer.Instance == null)
            {
                if (GUILayout.Button("Create Contract Addresses Container"))
                {
                    MarketplacePlasmachainContractAddressesPersistentContainer.CreateInstance();
                }

                return;
            }

            if (GUILayout.Button("Fetch Contract Addresses"))
            {
                MarketplacePlasmachainContractAddressChecker checker =
                    new MarketplacePlasmachainContractAddressChecker(_battlegroundMarketplaceSrcContractsPath);
                _contractAddressesNetworks = checker.GetContractAddressesOnAllNetworks();
            }

            if (_contractAddressesNetworks == null)
                return;

            if (GUILayout.Button("Approve Fetched Addresses"))
            {
                if (EditorUtility.DisplayDialog(
                    "Approve Fetched Addresses",
                    "Are you sure those addresses are okay and propagated to zbversion?",
                    "Approve",
                    "No Way."
                ))
                {
                    MarketplacePlasmachainContractAddressesPersistentContainer.Instance.ContractAddressesNetworks = _contractAddressesNetworks;
                    _ownerWindow.ShowNotification(new GUIContent("Approved!"));
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Stored Addresses", EditorStyles.helpBox);
                    DrawContractAddressesNetworks(
                        _contractAddressesNetworks,
                        MarketplacePlasmachainContractAddressesPersistentContainer.Instance.ContractAddressesNetworks
                    );
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Fetched Addresses", EditorStyles.helpBox);
                    DrawContractAddressesNetworks(
                        MarketplacePlasmachainContractAddressesPersistentContainer.Instance.ContractAddressesNetworks,
                        _contractAddressesNetworks
                    );
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContractAddressesNetworks(
            MarketplacePlasmachainContractAddressesNetworks drawnNetworks,
            MarketplacePlasmachainContractAddressesNetworks otherNetworks)
        {
            void DrawAddress(string name, string address, string otherAddress)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUIStyle labelStyle = EditorStyles.label;
                    Color color = GUI.contentColor;
                    if (address != otherAddress)
                    {
                        GUI.contentColor = Color.red;
                        labelStyle = EditorStyles.whiteLabel;
                    }

                    EditorGUILayout.LabelField(name + ": ", labelStyle, GUILayout.Width(140));
                    EditorGUILayout.SelectableLabel(address, labelStyle, GUILayout.Height(16));
                    GUI.contentColor = color;
                }
                EditorGUILayout.EndHorizontal();
            }

            foreach (MarketplacePlasmachainNetwork network in MarketplacePlasmachainContractAddressChecker.Networks)
            {
                GUILayout.Space(15);
                MarketplacePlasmachainNetworkContractAddresses addresses = drawnNetworks[network];
                MarketplacePlasmachainNetworkContractAddresses otherAddresses = otherNetworks[network];
                string niceNetworkName;
                switch (network)
                {
                    case MarketplacePlasmachainNetwork.Development:
                        niceNetworkName = "Development, test-z-asia1.dappchains.com (asia1)";
                        break;
                    case MarketplacePlasmachainNetwork.Staging:
                        niceNetworkName = "Staging, test-z-us1.dappchains.com (stage, us1)";
                        break;
                    case MarketplacePlasmachainNetwork.Production:
                        niceNetworkName = "Production, plasma.dappchains.com (plasma)";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(niceNetworkName, EditorStyles.boldLabel, GUILayout.Width(350));
                if (GUILayout.Button("Copy zbversion Template", GUILayout.ExpandWidth(false)))
                {
                    PlasmachainEndpointsConfiguration endpointsConfiguration = new PlasmachainEndpointsConfiguration(
                        "DUMMMY",
                        "DUMMMY",
                        "DUMMMY",
                        addresses.ZbgCardContractAddress,
                        addresses.CardFaucetContractAddress,
                        addresses.BoosterPackContractAddress,
                        addresses.SuperPackContractAddress,
                        addresses.AirPackContractAddress,
                        addresses.EarthPackContractAddress,
                        addresses.FirePackContractAddress,
                        addresses.LifePackContractAddress,
                        addresses.ToxicPackContractAddress,
                        addresses.WaterPackContractAddress,
                        addresses.SmallPackContractAddress,
                        addresses.MinionPackContractAddress,
                        addresses.BinancePackContractAddress,
                        addresses.FiatPurchaseContractAddress,
                        addresses.OpenLotteryContractAddress,
                        addresses.TronLotteryContractAddress
                    );

                    EditorGUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(endpointsConfiguration, Formatting.Indented);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                DrawAddress("ZbgCard", addresses.ZbgCardContractAddress, otherAddresses.ZbgCardContractAddress);
                DrawAddress("CardFaucet", addresses.CardFaucetContractAddress, otherAddresses.CardFaucetContractAddress);
                DrawAddress("BoosterPack", addresses.BoosterPackContractAddress, otherAddresses.BoosterPackContractAddress);
                DrawAddress("SuperPack", addresses.SuperPackContractAddress, otherAddresses.SuperPackContractAddress);
                DrawAddress("AirPack", addresses.AirPackContractAddress, otherAddresses.AirPackContractAddress);
                DrawAddress("EarthPack", addresses.EarthPackContractAddress, otherAddresses.EarthPackContractAddress);
                DrawAddress("FirePack", addresses.FirePackContractAddress, otherAddresses.FirePackContractAddress);
                DrawAddress("LifePack", addresses.LifePackContractAddress, otherAddresses.LifePackContractAddress);
                DrawAddress("ToxicPack", addresses.ToxicPackContractAddress, otherAddresses.ToxicPackContractAddress);
                DrawAddress("WaterPack", addresses.WaterPackContractAddress, otherAddresses.WaterPackContractAddress);
                DrawAddress("SmallPack", addresses.SmallPackContractAddress, otherAddresses.SmallPackContractAddress);
                DrawAddress("MinionPack", addresses.MinionPackContractAddress, otherAddresses.MinionPackContractAddress);
                DrawAddress("BinancePack", addresses.BinancePackContractAddress, otherAddresses.BinancePackContractAddress);
                DrawAddress("FiatPurchase", addresses.FiatPurchaseContractAddress, otherAddresses.FiatPurchaseContractAddress);
                DrawAddress("OpenLottery", addresses.OpenLotteryContractAddress, otherAddresses.OpenLotteryContractAddress);
                DrawAddress("TronLottery", addresses.TronLotteryContractAddress, otherAddresses.TronLotteryContractAddress);

                EditorGUI.indentLevel--;
            }
        }
    }
}
