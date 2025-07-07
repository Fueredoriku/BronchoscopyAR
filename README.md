# Bronchoscopy navigation Augmented Reality tool
A tool for visualizing bronchial tree and surrounding anatomy segmented in Fraxinus in augmented reality for better depth perception and navigation.

# Motivation
Bronchoscopy is a hard procedure to master, as clinicians have to create a mental
map of a complex bronchial tree structure in order to properly navigate, which
requires a lot of hands-on time with experts to learn. Some existing visualization
tools like Fraxinus can produce visual routes within a bronchial tree on a 2D
screen to help with intraoperative navigation. This thesis aims to create a 3D AR
bronchial tree visualization using the data from Fraxinus in order to understand
if there is any potential benefits of using such a visualization for bronchoscopy
compared to traditional 2D screen visualizations for bronchoscopy.

### Build and deploy

To build this application and manually deploy to hololens you need to the following:

*prerequisites*
Download the necessary packages from the visual studio installer, mainly those releated to the Universal Windows Platfom and connection over USB

1. Go to build settings in Unity and select Universal Windows Platform.
2. Select local machine, and build for ARM64. Set the build to "release".
   This will create a new visual studio solution in the selected build folder.
4. Open the built solution with viusal studio. Select "Release", "ARM64" and "Local Machine" in the build options.
5. Run build solution in Build -> Build Solution
6. Open: Project -> Publish -> Create app packages
7. Choose sideloading, auto generate and add certificate, and choose only the ARMx64 checkbox.
8. After build and packaging completes, you can now manually upload the generated BronchoscopyAR.appx file through the hololens device portal.
- Note: If you get an .msix, the network permissions will not be correct, so you must downgrade your build tools!
