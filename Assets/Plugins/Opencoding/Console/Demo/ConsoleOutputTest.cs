using Opencoding.CommandHandlerSystem;
using UnityEngine;

#if UNITY_5
using UnityEngine.Assertions;
#endif

namespace Opencoding.Console.Demo
{
	/// <summary>
	/// This class outputs some fake console messages on startup. It's just designed so that
	/// the console isn't completely empty when the demo game starts.
	/// </summary>
	public class ConsoleOutputTest : MonoBehaviour
	{
		// Thanks to SimCity for these :)
		private static string[] _randomStartupText =
		{
			"Adding Hidden Agendas", "Adjusting Bell Curves",
			"Aesthesizing Industrial Areas", "Aligning Covariance Matrices", "Applying Feng Shui Shaders",
			"Applying Theatre Soda Layer", "Asserting Packed Exemplars", "Attempting to Lock Back-Buffer",
			"Binding Sapling Root System", "Breeding Fauna", "Building Data Trees", "Bureacritizing Bureaucracies",
			"Calculating Inverse Probability Matrices", "Calculating Llama Expectoration Trajectory", "Calibrating Blue Skies",
			"Charging Ozone Layer", "Coalescing Cloud Formations", "Cohorting Exemplars", "Collecting Meteor Particles",
			"Compounding Inert Tessellations", "Compressing Fish Files", "Computing Optimal Bin Packing",
			"Concatenating Sub-Contractors", "Containing Existential Buffer", "Debarking Ark Ramp",
			"Debunching Unionized Commercial Services", "Deciding What Message to Display Next", "Decomposing Singular Values",
			"Decrementing Tectonic Plates", "Deleting Ferry Routes", "Depixelating Inner Mountain Surface Back Faces",
			"Depositing Slush Funds", "Destabilizing Economic Indicators", "Determining Width of Blast Fronts",
			"Deunionizing Bulldozers", "Dicing Models", "Diluting Livestock Nutrition Variables",
			"Downloading Satellite Terrain Data", "Exposing Flash Variables to Streak System", "Extracting Resources",
			"Factoring Pay Scale", "Fixing Election Outcome Matrix", "Flood-Filling Ground Water", "Flushing Pipe Network",
			"Gathering Particle Sources", "Generating Jobs", "Gesticulating Mimes", "Graphing Whale Migration",
			"Hiding Willio Webnet Mask", "Implementing Impeachment Routine", "Increasing Accuracy of RCI Simulators",
			"Increasing Magmafacation", "Initializing My Sim Tracking Mechanism", "Initializing Rhinoceros Breeding Timetable",
			"Initializing Robotic Click-Path AI", "Inserting Sublimated Messages", "Integrating Curves",
			"Integrating Illumination Form Factors", "Integrating Population Graphs", "Iterating Cellular Automata",
			"Lecturing Errant Subsystems", "Mixing Genetic Pool", "Modeling Object Components", "Mopping Occupant Leaks",
			"Normalizing Power", "Obfuscating Quigley Matrix", "Overconstraining Dirty Industry Calculations",
			"Partitioning City Grid Singularities", "Perturbing Matrices", "Pixalating Nude Patch", "Polishing Water Highlights",
			"Populating Lot Templates", "Preparing Sprites for Random Walks", "Prioritizing Landmarks",
			"Projecting Law Enforcement Pastry Intake", "Realigning Alternate Time Frames", "Reconfiguring User Mental Processes",
			"Relaxing Splines", "Removing Road Network Speed Bumps", "Removing Texture Gradients",
			"Removing Vehicle Avoidance Behavior", "Resolving GUID Conflict", "Reticulating Splines", "Retracting Phong Shader",
			"Retrieving from Back Store", "Reverse Engineering Image Consultant", "Routing Neural Network Infanstructure",
			"Scattering Rhino Food Sources", "Scrubbing Terrain", "Searching for Llamas",
			"Seeding Architecture Simulation Parameters", "Sequencing Particles", "Setting Advisor Moods",
			"Setting Inner Deity Indicators", "Setting Universal Physical Constants", "Sonically Enhancing Occupant-Free Timber",
			"Speculating Stock Market Indices", "Splatting Transforms", "Stratifying Ground Layers", "Sub-Sampling Water Data",
			"Synthesizing Gravity", "Synthesizing Wavelets", "Time-Compressing Simulator Clock",
			"Unable to Reveal Current Activity", "Weathering Buildings", "Zeroing Crime Network"
		};

		private void Start()
		{
			PerformFakeStartupFlow();

			CommandHandlers.RegisterCommandHandlers(this);
		}

		// This outputs some fake messages on startup. It's just designed to show the console with some content in it.
		private static void PerformFakeStartupFlow()
		{
			for (int i = 0; i < 10; ++i)
			{
				Debug.Log(_randomStartupText[Random.Range(0, _randomStartupText.Length)]);
				switch (Random.Range(0, 5))
				{
					case 3:
						Debug.LogError("Failed");
						break;
					case 4:
						Debug.LogWarning("Something went wrong");
						break;
				}

#if UNITY_5
	// Debug.Assert is only supported on Unity 5 and above
				Debug.Assert(i % 5 == 0, string.Format("{0} is not divisible by 5", i));
#endif
			}

			Debug.Log("Startup is complete!");
		}
	}
}