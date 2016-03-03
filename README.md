# Curve-Smoothing
An algorithm to eliminate noise from free hand drawing in C# 

Implements Douglas-Peucker curve fitting algorithm to obtain the minimum number of points that can represent the curve drawn by the user. After the reduction, a Catmull-Romm spline is drawn through those points resulting in a smooth curve.
