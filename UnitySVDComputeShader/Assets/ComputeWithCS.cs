using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class ComputeWithCS : MonoBehaviour {
    
    [SerializeField] private ComputeShader cs;
    ComputeBuffer input_buffer_;
    Input[] input_data_;

    ComputeBuffer output_buffer_;
    Output[] output_data_;

    int kernal_id;

    [SerializeField] int number_of_buffer_ = 10000;
    float range = 1000;

    int test_count_ = 0;
    int error_count_ = 0;

    float time_total_ = 0;
    float time_count_ = 0;
    float current_time_ = 0;

    struct Input
    {
        public Matrix4x4 A;
    }

    struct Output
    {
        public Matrix4x4 U;
        public Vector4 D;
        public Matrix4x4 Vt;
    }

    // Use this for initialization
    void Start () {
        input_buffer_  = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(Input)));
        output_buffer_ = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(Output)));

        this.input_data_= new Input[number_of_buffer_];
        this.RandomInputAndSetToBuffer();

        this.output_data_ = new Output[number_of_buffer_];

        this.kernal_id = cs.FindKernel("ComputeSVD3D");


        this.RunCS();
    }

    // Update is called once per frame
    private void Update()
    {
        this.RunCS();
    }

    void RandomInputAndSetToBuffer()
    {
        test_count_ += this.input_data_.Length;
        for (int i = 0; i < this.input_data_.Length; i++)
        {
            //generator some random matrix
            this.input_data_[i].A = new Matrix4x4(
                new Vector4(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range), 0),
                new Vector4(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range), 0),
                new Vector4(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range), 0),
                new Vector4(0, 0, 0, 0));
        }
        input_buffer_.SetData(input_data_);
    }

    void RunCS ()
    {
        time_count_++;
        this.RandomInputAndSetToBuffer();

        if (cs != null)
        {
            cs.SetBuffer(this.kernal_id, "_input_buffer", input_buffer_);
            cs.SetBuffer(this.kernal_id, "_output_buffer", output_buffer_);

            float time = Time.realtimeSinceStartup;
            Profiler.BeginSample("SVD3D");
            cs.Dispatch(this.kernal_id, number_of_buffer_/8, number_of_buffer_/8, 1);
            Profiler.EndSample();

            current_time_ = Time.realtimeSinceStartup - time;
            time_total_ += current_time_;

            output_buffer_.GetData(output_data_);

            VerifyData(input_data_, output_data_);
        }

	}

    void VerifyData(Input[] input, Output[] output)
    {
        for (int i = 0; i < input.Length; ++i)
        {
            Matrix4x4 matirx_d = new Matrix4x4(
                new Vector4(output[i].D[0], 0, 0, 0),
                new Vector4(0, output[i].D[1], 0, 0),
                new Vector4(0, 0, output[i].D[2], 0),
                new Vector4(0, 0, 0, 0)
            );

            Matrix4x4 delta = output[i].U * matirx_d * output[i].Vt;
            delta.SetRow(0, input[i].A.GetRow(0) - delta.GetRow(0));
            delta.SetRow(1, input[i].A.GetRow(1) - delta.GetRow(1));
            delta.SetRow(2, input[i].A.GetRow(2) - delta.GetRow(2));
            delta.SetRow(3, input[i].A.GetRow(3) - delta.GetRow(3));

            //Debug.LogFormat("Verifying {0} with \n {1}", input[i].A, delta);

            bool checked_value = false;
            for (int j = 0; j < 3 && !checked_value; ++j)
            { 
                for (int k = 0; k < 3; ++k)
                {
                    if (Mathf.Abs(delta[j,k]) > 0.0005 * range)
                    {
                        Debug.LogWarningFormat("A is \n{4}\n" +
                            "Retored is \n{5}\n" +
                            "output is \n" +
                            "U:\n{0}\n" +
                            "D:\n{1}\n" +
                            "Vt:\n{2}\n" +
                            "Delta is \n{3}\n", output[i].U, matirx_d, output[i].Vt, delta, input[i].A, output[i].U * matirx_d * output[i].Vt);

                        checked_value = true;
                        error_count_++;
                        break;
                    }
                }
            }

            //Assert.IsTrue(input[i].A == output[i].U * matirx_d * output[i].Vt);
        }
    }
    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 512 , 512), result_);

        string s = System.String.Format("input tested: {0}\nerror ratio {1:P4}\ntime average: {2:F5} ms\nCurrent Time:{3:F5}", 
                                        test_count_, (error_count_ * 1.0f / test_count_), time_total_/time_count_ * 1000, current_time_ * 1000f);
        GUI.TextArea(new Rect(100, 100, 300, 100), s);
    }
}
