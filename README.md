# UnitySVDComputeShader

This is a Unity implementation of [Implicit-shifted Symmetric QR Singular Value Decomposition of 3x3 Matrices](http://www.math.ucla.edu/~fuchuyuan/svd/svd.html)

As a part of [A material point method for snow simulation](https://www.math.ucla.edu/~jteran/papers/SSCTS13.pdf).

### It is a GPU SVD implementation, I use compute shader to get SVD for both 2D and 3D matrix.
it takes about 171 ms GPU time to calculate svd values of 10240 matrices. 
and takes about 0.23 ms GPU time to calculate svd values of 256 matrices.
