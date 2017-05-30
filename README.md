# SparkyKit
System for creating sparks, trails, and fireworks in the Unity game engine.

## Classes

### SparkyEmitter
Component that creates sparks.  Will destroy itself after the parent is destroyed, but sparks will fade away gracefully.

### SparkyTrail
A trail that follows behind an object.  Will automatically destroy itself after the parent is destroyed and will fade the trail gracefully.

## Other Classes

### SparkySpark
Sparks from a SparkyEmitter.

### Spline
Simple spline implementation for smooth curve evaluation.