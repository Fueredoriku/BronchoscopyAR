# Interactive augmented reality depth debugger with sphere tracing
Drag a plane through 3D space to investigate the distance field between the hololens depth buffer and virtualy rendered SDFs.
This tool enables debugging of how the distance fields in a composite scene interacts so one can avoid unwanted artifacts in the SDFs.

# Motivation
- Debugging distance fields without any visual cues is pretty hard, and XR is a good way of getting much needed depth perception.
- Having SDFs interact with a composite scene enables SDFs that can interact in a convincing way with an augmented reality environmnent.
   - An example could be to get an SDF to slightly meld into the hololens generated point cloud geometry.

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
8. After build and packaging completes, you can now manually upload the generated SDF-AR.appx file through the hololens device portal.
# BronchoscopyAR
