using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace City.View
{
    public class FogView : MonoBehaviour
    {
        [Header("解锁迷雾速度")]
        public float UnlockFogSpeed = 5f;
        //数据
        private float[] unlockData;//存放已解锁的序号
        private int highlight_Index = -1;//当且点击高光的区域序号，无则为-1
        private int[,] fogIdx = null;
        private int indexDataId = 0;
        private int maxW = 0;
        private int maxH = 0;
        private Vector2Int[] areaCenter = null;
        //图形与材质
        [SerializeField]
        public Texture2D tex;
        
        private Material curMat;

        //选取对应激活区域
        public Shader getActiveIndexShader = null;
        //对解锁结果进行更新(R通道)
        public Shader blendShader;

        //实现高斯模糊
        public Shader blurShader = null;
        public int blurTimes = 2;

        //RT
        public RenderTexture retRT_active;
        public RenderTexture retRT_highlight;
        private RenderTexture rt_getIndex_r;
        private RenderTexture rt_gethighlight;
        private RenderTexture rt_blend;
        private RenderTexture rt0;
        private RenderTexture rt1;

        //Material
        private Material blurMat;
        private Material blendMat;
        private Material indexMat;

        private void Awake()
        {
            InitFog();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                UpdateUnlockData(1, 1);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                UpdateUnlockData(2, 1);
            }
        }

        public void InitFog()
        {
            indexDataId = Shader.PropertyToID("indexData");
            maxW = 200 + 30 * 2;
            maxH = 200 + 30 * 2;
            InitFogRenderData();
            InitFogArea();
        }

        private void InitFogRenderData()
        {
            //二进制文件转成texture2D
            string wayPointPath = "Assets/Fog/conf/fog_flag.bytes";
            byte[] content = File.ReadAllBytes(wayPointPath);
            tex = new Texture2D(maxW, maxH, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            tex.filterMode = FilterMode.Point;
            tex.LoadRawTextureData(content);
            tex.Apply();

            //初始化rt
            retRT_active = new RenderTexture(maxW, maxH, 0, RenderTextureFormat.R8);
            retRT_highlight = new RenderTexture(maxW, maxH, 0, RenderTextureFormat.R8);
            rt_getIndex_r = new RenderTexture(maxW, maxH, 0, RenderTextureFormat.R8);
            rt_blend = new RenderTexture(maxW, maxH, 0, RenderTextureFormat.R8);
            rt_gethighlight = new RenderTexture(maxW, maxH, 0, RenderTextureFormat.R8);
            rt0 = RenderTexture.GetTemporary(maxW, maxH, 0, RenderTextureFormat.R8);
            rt1 = RenderTexture.GetTemporary(maxW, maxH, 0, RenderTextureFormat.R8);

            //初始化Material
            blurMat = new Material(blurShader);
            blendMat = new Material(blendShader);
            indexMat = new Material(getActiveIndexShader);

            unlockData = new float[256];
            for (int i = 0; i < unlockData.Length; ++i)
            {
                unlockData[i] = 0;
            }
            areaCenter = new Vector2Int[256];
            for (int i = 0; i < 256; ++i)
            {
                areaCenter[i] = Vector2Int.zero;
            }
            curMat = GetComponent<Renderer>().material;
        }

        private void InitFogArea()
        {
            int w = maxW;
            int h = maxH;
            fogIdx = new int[w, h];
            if (tex == null)
            {
                return;
            }

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color c = tex.GetPixel(i, j);
                    fogIdx[i, j] = (int)(c.r * 255);
                    if (c.g != 0)
                    {
                        areaCenter[(int)(c.g * 255)] = new Vector2Int(i, j);
                    }
                }
            }
        }

        public int GetFogAreaByXY(int x, int y)
        {
            int w = maxW;
            int h = maxH;
            if (fogIdx == null)
            {
                return -1;
            }

            if (areaCenter == null)
            {
                return -1;
            }
            if (x >= 0 && y >= 0 & x < w && y < h)
            {
                return fogIdx[x, y];
            }

            return -1;
        }

        /// <summary>
        /// 更新数据：1~100R通道数据 101~200G通道数据
        /// 输入时G通道为原先值加100
        /// 输入参数 index 开启指定序号，默认为-1（不开启）
        /// 输入参数 index_value 决定index开启或关闭，开启为1，关闭为0
        /// </summary>
        /// <param name="index"></param>
        public void UpdateUnlockData(int index = -1, int index_value = 0)
        {
            unlockData[index] = index_value;
            if(index != -1)GenerateBlinearTex(0,index);
        }
        //更新高光区域，输入参数为该区域序号
        public void UpdateCurrentHighlightArea(int index)
        {
            if (highlight_Index == index)
            {
                return;
            }
            
            if (index < 0)
            {
                GenerateBlinearTex(1,highlight_Index);
                highlight_Index = index;
            }
            else
            {
                highlight_Index = index;
                GenerateBlinearTex(1,highlight_Index);
            }
        }

        public void Release()
        {
            if (tex != null)
            {
                GameObject.DestroyImmediate(tex);
            }
            if (rt_blend != null) GameObject.DestroyImmediate(rt_blend);
            if (rt_getIndex_r != null) GameObject.DestroyImmediate(rt_getIndex_r);
            if (retRT_active != null) GameObject.DestroyImmediate(retRT_active);
            if (retRT_highlight != null) GameObject.DestroyImmediate(retRT_highlight);
            if (rt0 != null) RenderTexture.ReleaseTemporary(rt0);
            if (rt1 != null) RenderTexture.ReleaseTemporary(rt1);
            if (blendMat != null) GameObject.DestroyImmediate(blendMat);
            if (blurMat != null) GameObject.DestroyImmediate(blurMat);
            if (indexMat != null) GameObject.DestroyImmediate(indexMat);
        }
        //模糊处理
        private RenderTexture Blur(RenderTexture _tex)
        {
            blurMat.SetVector("_TexSize", new Vector4(maxW, maxH));
            Graphics.Blit(_tex, rt0);
            for (int i = 0; i < blurTimes; ++i)
            {
                Graphics.Blit(rt0, rt1, blurMat, 0);
                Graphics.Blit(rt1, rt0, blurMat, 1);
            }
            Graphics.Blit(rt0, _tex);
            return _tex; 
        }
        private void Blend()
        {
            blendMat.SetTexture("_CurTex", rt_getIndex_r);
            Graphics.Blit(rt_blend, retRT_active, blendMat, 0);
            Graphics.Blit(retRT_active, rt_blend);
        }
        private void GenerateBlinearTex(int channel, int index)
        {
            //解锁区域操作
            if (channel == 0)
            {
                //indexMat.SetFloat("_Area", 0.5f);
                //Graphics.Blit(tex, rt_getIndex_r, indexMat, 0);
                //Blend();
                //curMat.SetTexture("_BlurTexRuntime",Blur(retRT_active));
                StartCoroutine(UnlockFog(index));
            }
            else if (channel == 1 && index>=0 &&index<unlockData.Length && Math.Abs(unlockData[index]) < 0.001)
            {
                indexMat.SetInt("_Index", index);
                indexMat.SetTexture("_LastTex", rt_gethighlight);
                indexMat.SetFloat("_Area", 100000f);
                Graphics.Blit(tex, retRT_highlight, indexMat, 0);
                Graphics.Blit(retRT_highlight, rt_gethighlight);
                curMat.SetTexture("_BlurTexRuntime_Limit", Blur(retRT_highlight));
            }
        }

        IEnumerator UnlockFog(int index)
        {
            for (float tmp = 0f; tmp <= 1.0f; tmp += Time.deltaTime * 0.01f * UnlockFogSpeed)
            {
                indexMat.SetInt("_Index", index);
                indexMat.SetVector("_CenterPoint", new Vector4(areaCenter[index].x/(float)maxW, areaCenter[index].y/(float)maxH,0,0));
                
                indexMat.SetFloat("_Area", tmp);
                Graphics.Blit(tex, rt_getIndex_r, indexMat, 0);
                Blend();
                curMat.SetTexture("_BlurTexRuntime",Blur(retRT_active));
                yield return 0f;
            }
            
        }
    }
}
