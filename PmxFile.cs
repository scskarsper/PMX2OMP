using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PMDEditor;
using System.IO;
using System.Windows.Forms;

namespace PMXCheckerForOMP
{
    public class PmxFile
    {
        public Pmx GetFile(string FilePath)
        {
            Pmx Ret = new Pmx();
            using (FileStream fileStream = new FileStream(FilePath, FileMode.Open))
            {
                try
                {
                    Ret = FromStreamEx(fileStream,null);
                }
                catch(Exception e)
                {
                    if(MessageBox.Show("读取异常，不能继续下去了", "确认")==DialogResult.OK || true)
                    {
                                            throw;
                    }
                }
            }
            return Ret;
        }
        
        // PMDEditor.Pmx
        public Pmx FromStreamEx(Stream s, PmxElementFormat f=null)
        {
            Pmx Ret = new Pmx();
            PmxHeader pmxHeader = new PmxHeader(2f);
            pmxHeader.FromStreamEx(s, null);
            Ret.Header = pmxHeader;
            if (pmxHeader.Ver <= 1f)
            {
                MMD_Pmd mMD_Pmd = new MMD_Pmd();
                s.Seek(0L, SeekOrigin.Begin);
                mMD_Pmd.FromStreamEx(s, null);
                Ret.FromPmx(PmxConvert.PmdToPmx(mMD_Pmd));
                return Ret;
            }
            Ret.ModelInfo = new PmxModelInfo();
            Ret.ModelInfo.FromStreamEx(s, pmxHeader.ElementFormat);
            int num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.VertexList = new List<PmxVertex>();
            Ret.VertexList.Clear();
            Ret.VertexList.Capacity = num;
            for (int i = 0; i < num; i++)
            {
                PmxVertex pmxVertex = new PmxVertex();
                pmxVertex.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.VertexList.Add(pmxVertex);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.FaceList = new List<int>();
            Ret.FaceList.Clear();
            Ret.FaceList.Capacity = num;
            for (int j = 0; j < num; j++)
            {
                int item = PmxStreamHelper.ReadElement_Int32(s, pmxHeader.ElementFormat.VertexSize, false);
                Ret.FaceList.Add(item);
            }
            PmxTextureTable pmxTextureTable = new PmxTextureTable();
            pmxTextureTable.FromStreamEx(s, pmxHeader.ElementFormat);
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.MaterialList = new List<PmxMaterial>();
            Ret.MaterialList.Clear();
            Ret.MaterialList.Capacity = num;
            for (int k = 0; k < num; k++)
            {
                PmxMaterial pmxMaterial = new PmxMaterial();
                pmxMaterial.FromStreamEx_TexTable(s, pmxTextureTable, pmxHeader.ElementFormat);
                Ret.MaterialList.Add(pmxMaterial);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.BoneList = new List<PmxBone>();
            Ret.BoneList.Clear();
            Ret.BoneList.Capacity = num;

            for (int l = 0; l < num; l++)
            {
                PmxBone pmxBone = new PmxBone();
                pmxBone.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.BoneList.Add(pmxBone);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.MorphList = new List<PmxMorph>();
            Ret.MorphList.Clear();
            Ret.MorphList.Capacity = num;
            for (int m = 0; m < num; m++)
            {
                PmxMorph pmxMorph = new PmxMorph();
                pmxMorph.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.MorphList.Add(pmxMorph);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.NodeList = new List<PmxNode>();
            Ret.NodeList.Clear();
            Ret.NodeList.Capacity = num;
            for (int n = 0; n < num; n++)
            {
                PmxNode pmxNode = new PmxNode();
                pmxNode.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.NodeList.Add(pmxNode);
                if (Ret.NodeList[n].SystemNode)
                {
                    if (Ret.NodeList[n].Name == "Root")
                    {
                        Ret.RootNode = Ret.NodeList[n];
                    }
                    else if (Ret.NodeList[n].Name == "表情")
                    {
                        Ret.ExpNode = Ret.NodeList[n];
                    }
                }
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.BodyList = new List<PmxBody>();
            Ret.BodyList.Clear();
            Ret.BodyList.Capacity = num;
            for (int num2 = 0; num2 < num; num2++)
            {
                PmxBody pmxBody = new PmxBody();
                pmxBody.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.BodyList.Add(pmxBody);
            }
            num = PmxStreamHelper.ReadElement_Int32(s, 4, true);
            Ret.JointList = new List<PmxJoint>();
            Ret.JointList.Clear();
            Ret.JointList.Capacity = num;
            for (int num3 = 0; num3 < num; num3++)
            {
                PmxJoint pmxJoint = new PmxJoint();
                pmxJoint.FromStreamEx(s, pmxHeader.ElementFormat);
                Ret.JointList.Add(pmxJoint);
            }
            return Ret;
        }

        public PmxBone CreateNewBone()
        {
            PmxBone b = new PmxBone();
            b.AppendParent = -1;
            b.AppendRatio = 1;
            b.Axis.X = 0;
            b.Axis.Y = 0;
            b.Axis.Z = 0;
            b.ExtKey = 0;
            b.Flags = PmxBone.BoneFlags.Rotation | PmxBone.BoneFlags.Enable;
            b.IK = new PmxIK();
            b.IK.Angle = 1;
            b.IK.LinkList = new List<PmxIK.IKLink>();
            b.IK.Target = -1;
            b.IKKind = PmxBone.IKKindType.None;
            b.Level = 0;
            b.LocalX.X = 1;
            b.LocalY.Y = 1;
            b.LocalZ.Z = 1;
            b.To_Bone = -1;
            b.To_Offset.X = 0;
            b.To_Offset.Y = 0;
            b.To_Offset.Z = 0;
            return b;
        }
    }
}
