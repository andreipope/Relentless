using System;
using KellermanSoftware.CompareNetObjects;
using Loom.ZombieBattleground.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor.Build
{
    public class PlasmachainContractAddressesCheckerPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            string contractsRoot = EditorPrefs.GetString("MarketplaceContractAddressCheck_BattlegroundMarketplaceSrcContractsPathPrefs", "");
            if (String.IsNullOrEmpty(contractsRoot))
            {
                contractsRoot = Environment.GetEnvironmentVariable("RL_MARKETPLACE_CONTRACTS_ROOT");
                if (String.IsNullOrEmpty(contractsRoot))
                    return;
            }

            if (!MarketplacePlasmachainContractAddressChecker.IsValidContractsRootDirectory(contractsRoot))
            {
                Debug.LogWarning("Invalid contracts root directory: " + contractsRoot);
                return;
            }

            MarketplacePlasmachainContractAddressChecker checker = new MarketplacePlasmachainContractAddressChecker(contractsRoot);
            MarketplacePlasmachainContractAddressesNetworks fetchedNetworks = checker.GetContractAddressesOnAllNetworks();
            ComparisonResult comparisonResult =
                checker.CompareNetworks(
                    MarketplacePlasmachainContractAddressesPersistentContainer.Instance.ContractAddressesNetworks,
                    fetchedNetworks);

            if (!comparisonResult.AreEqual)
                throw new OperationCanceledException("Stored and current PlasmaChain contract addresses mismatch:\n" + comparisonResult.DifferencesString);
        }
    }
}
