using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleParticles : MonoBehaviour {

	// Particle data
    private struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;

    }


    //size of particle struct
    private const int PART_SIZE = 24;


    public int numParticles = 1000;

    public Material material;

    //Computshader
    public ComputeShader cs;

    //Particle buffer
    ComputeBuffer particleBuffer;

    //Kernel ID

    private int mCsKernelID;

    //Block size
    private const int BLOCK = 256;

    public GameObject BlackHole1;

    public GameObject BlackHole2;

    //number of blocks needed
    private int blockCount;
    private void Start()
    {

        if (numParticles <= 0)
        {
            numParticles = 1;
        }
        blockCount = Mathf.CeilToInt((float)numParticles / BLOCK);

        Particle[] particleArray = new Particle[numParticles];
        for (int i = 0; i < numParticles; i++)
        {
            particleArray[i].position.x = Random.value * 3 - 2.0f;
            particleArray[i].position.y = Random.value * 3 - 2.0f;
            particleArray[i].position.z = Random.value * 3 - 2.0f;

            particleArray[i].velocity.x = 0;
            particleArray[i].velocity.y = 0;
            particleArray[i].velocity.z = 0;
        }

        //Make the compute buffer holding the Particle
        particleBuffer = new ComputeBuffer(numParticles, PART_SIZE);
        particleBuffer.SetData(particleArray);

        // get kernelID
        mCsKernelID = cs.FindKernel("CSMain");

        //Bind shader and compute shader to the buffer of particles
        cs.SetBuffer(mCsKernelID, "particleBuffer", particleBuffer);
        material.SetBuffer("particleBuffer", particleBuffer);

    }

    private void OnDestroy()
    {
        if (particleBuffer != null)
        {
            particleBuffer.Release();
        }
    }

    // Update is called once per frame
    void Update () {

        Vector3 bhPos1 = BlackHole1.transform.position;
        float[] blackHolePosition1 = { bhPos1.x, bhPos1.y, bhPos1.z };


        Vector3 bhPos2 = BlackHole2.transform.position;
        float[] blackHolePosition2 = { bhPos2.x, bhPos2.y, bhPos2.z };

        //Send info/data to compute shader
        cs.SetFloats("blackHolePosition1", blackHolePosition1);
        cs.SetFloats("blackHolePosition2", blackHolePosition2);
        cs.SetFloat("deltaTime", Time.deltaTime);

        //UpdateParticles
        cs.Dispatch(mCsKernelID, blockCount, 1, 1);

    }

    private void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, 1, numParticles);
    }
}
