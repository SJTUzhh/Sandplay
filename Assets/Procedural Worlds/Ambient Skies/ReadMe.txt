Ambient Skies
=============

For more information on how to get started with Ambient Skies please read the Quick Start Guide in the documentation directory.

Notes:

1. If this error ever presents itself: 
"Setting anti-aliasing of already created render texture is not supported!UnityEngine.GUIUtility:ProcessEvent(Int32, IntPtr)"
 
To fix:
Close and reopen Unity, and then remove and reinstall Post Processing via the Package Manager.

2. If you have switched pipelines it is always a good ideas to clear your baked lightmaps data to remove old and incompatible data. 

To do this:
Open Ambient Skies and select the Lighting tab and then the Main settings panel and then click Clear Baked Lightmaps. 

3. If using Unity 2019 and you create a LWRP project, then switching from LWRP to another render pipeline will break Post Processing.

To fix:
Use the Package Manager to install the Post Processing package. 