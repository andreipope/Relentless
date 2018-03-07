using UnityEngine;
using System.Collections;

namespace I2.Loc
{
	#if SVG
	public partial class Localize
	{
		#region Cache

		SVGImporter.SVGImage 	mTarget_SVGImage;
		SVGImporter.SVGRenderer	mTarget_SVGRenderer;

		public void RegisterEvents_SVG()
		{
			EventFindTarget += FindTarget_SVGImage;
			EventFindTarget += FindTarget_SVGRenderer;
		}

		#endregion

		#region Find Target
		
		void FindTarget_SVGImage() 	{ FindAndCacheTarget (ref mTarget_SVGImage, SetFinalTerms_SVGImage, DoLocalize_SVGImage, true, false, false); }
		void FindTarget_SVGRenderer() 	{ FindAndCacheTarget (ref mTarget_SVGRenderer, SetFinalTerms_SVGRenderer, DoLocalize_SVGRenderer, true, false, false); }

		#endregion

		#region SetFinalTerms

		void SetFinalTerms_SVGImage(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			string primary = (mTarget_SVGImage.vectorGraphics!=null ? mTarget_SVGImage.vectorGraphics.name : string.Empty);
			string second = (mTarget_SVGImage.material!=null ? mTarget_SVGImage.material.name : string.Empty);
			SetFinalTerms (primary, second,		out primaryTerm, out secondaryTerm, true);
		}
		
		void SetFinalTerms_SVGRenderer(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			string primary = (mTarget_SVGRenderer.vectorGraphics!=null ? mTarget_SVGRenderer.vectorGraphics.name : string.Empty);
			string second = (mTarget_SVGRenderer.opaqueMaterial!=null ? mTarget_SVGRenderer.opaqueMaterial.name : string.Empty);
			SetFinalTerms (primary, second,		out primaryTerm, out secondaryTerm, true);
		}

		#endregion

		#region DoLocalize		
		
		public void DoLocalize_SVGImage(string MainTranslation, string SecondaryTranslation)
		{
			var OldVectorG = mTarget_SVGImage.vectorGraphics;
			if (OldVectorG==null || OldVectorG.name!=MainTranslation)
				mTarget_SVGImage.vectorGraphics = FindTranslatedObject<SVGImporter.SVGAsset>(MainTranslation);
			
			var OldMaterial = mTarget_SVGImage.material;
			if (OldMaterial==null || OldMaterial.name!=SecondaryTranslation)
				mTarget_SVGImage.material = FindTranslatedObject<Material>(SecondaryTranslation);

			mTarget_SVGImage.SetAllDirty ();
		}

		public void DoLocalize_SVGRenderer(string MainTranslation, string SecondaryTranslation)
		{
			var OldVectorG = mTarget_SVGRenderer.vectorGraphics;
			if (OldVectorG==null || OldVectorG.name!=MainTranslation)
				mTarget_SVGRenderer.vectorGraphics = FindTranslatedObject<SVGImporter.SVGAsset>(MainTranslation);
			
			var OldMaterial = mTarget_SVGRenderer.opaqueMaterial;
			if (OldMaterial==null || OldMaterial.name!=SecondaryTranslation)
				mTarget_SVGRenderer.opaqueMaterial = FindTranslatedObject<Material>(SecondaryTranslation);
			
			mTarget_SVGRenderer.SetAllDirty ();
		}
		#endregion
	}
	#else
	public partial class Localize
	{
		public static void RegisterEvents_SVG()
		{
		}
	}
	#endif	
}