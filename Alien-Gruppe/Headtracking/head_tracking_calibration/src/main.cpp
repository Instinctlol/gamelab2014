
// add windows.h if needed
#if OGRE_PLATFORM == OGRE_PLATFORM_WIN32
	#ifndef WIN32_LEAN_AND_MEAN
		#define WIN32_LEAN_AND_MEAN
	#endif
	#include "windows.h"
#endif

// main application class
#include "OgreBulletStereo.h"

// handle some system/compiler specific issues
// -- DO NOT EDIT -- 
#if !defined(OGRE_SHOW_CONSOLE) && OGRE_PLATFORM == OGRE_PLATFORM_WIN32
	#define _MAIN_ROUTINE_ INT WINAPI WinMain( HINSTANCE hInst, HINSTANCE, LPSTR strCmdLine, INT )
	#define _REPORT_ERROR(a) MessageBox( NULL, (a).getFullDescription().c_str(), "An exception has occured!", MB_OK | MB_ICONERROR | MB_TASKMODAL)

	// if MVS - set the output target
	#if _MSC_VER > 100
		#pragma comment(linker, "/SUBSYSTEM:WINDOWS")
	#endif 
#else // either OGRE_SHOW_CONSOLE defined, or not WINDOWS(r)
	#define _MAIN_ROUTINE_ int main(int argc, char *argv[])
	#define _REPORT_ERROR(a) std::cerr << "An exception has occured: " << (a).getFullDescription().c_str() << std::endl
#endif
// -- END OF "DO NOT EDIT" --

//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------
// entry point for the program
_MAIN_ROUTINE_
{
    // Create application object
    OgreBulletStereo app;

    try 
	{
        app.go();
    } 
	catch(Ogre::Exception& e) 
	{
		_REPORT_ERROR(e);
    }

    return 0;
}

