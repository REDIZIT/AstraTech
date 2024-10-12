using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AstraTech
{
    public class Building_AstraBlueprintHolder : Building
    {
        public Thing BlueprintItem => blueprintItem;

        private Thing blueprintItem;
        public ThingComp_AstraBlueprint blueprint => blueprintItem.TryGetComp<ThingComp_AstraBlueprint>();

        public Building_AstraCore core;

        private StringBuilder b = new StringBuilder();

        private float animationTime = 0.5f;
        private Vector3[] spherePoints = new Vector3[CUBES_AMOUNT_PER_SIDE * CUBES_AMOUNT_PER_SIDE * CUBES_AMOUNT_PER_SIDE];

        /// <summary>
        /// Magic number of y coordinate to draw a Mesh above the Holder's texture but below Pawns
        /// </summary>
        private const float PARTICLE_DRAW_DEPTH = 5;
        private const float FAKE_Y_COEF = 0.75f;
        private const int CUBES_AMOUNT_PER_SIDE = 4;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            FillSherePointsArray(spherePoints);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref blueprintItem, nameof(blueprintItem));
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var e in blueprint.SpecialDisplayStats())
            {
                yield return e;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            
            Material mat = MaterialPool.MatFrom("particle_cube_1");
            var tex = ContentFinder<Texture2D>.Get("particle_cube_1");
            Vector2 texSize = new Vector2(1, tex.height / (float)tex.width);

            Vector3 worldLeftBottomCorner = Position.ToVector3();
            Vector3 size = def.size.ToVector3();

            Vector3 perspectivedSize = new Vector3(0.8f * size.x, 0, 0.5f * size.z);
            Vector3 perspectivedSizeOffset = (size - perspectivedSize) / 2f;

            int cubesCountPerSide = 4;
            //int totalCubesCount = Mathf.RoundToInt(Mathf.Repeat(Time.time * 3, cubesCountPerSide * cubesCountPerSide * cubesCountPerSide));
            int totalCubesCount = 64;

            Vector3 spacePerCube = perspectivedSize / cubesCountPerSide;

            Mesh mesh = MeshPool.GridPlane(texSize * Vector2.one * spacePerCube.x);
            Vector3 oddOffset = new Vector3(spacePerCube.x / 2f, 0, spacePerCube.z / 2f);

            
            Vector3 meshDepth = new Vector3(0, PARTICLE_DRAW_DEPTH + 1, 0);


            int i = 0;
            for (int y = 0; y < cubesCountPerSide; y++)
            {
                for (int z = cubesCountPerSide - 1; z >= 0; z--)
                {
                    for (int x = 0; x < cubesCountPerSide; x++)
                    {
                        i++;

                        // Calculate cube idle pos
                        float fakeY = FAKE_Y_COEF * spacePerCube.x * y;
                        Vector3 cubeIdleLocation = new Vector3(spacePerCube.x * x, 0, spacePerCube.z * z + fakeY);

                        Vector3 perspectiveDepth = new Vector3(0, cubesCountPerSide - z, 0) * 0.01f;
                        
                        Vector3 drawIdlePos = perspectivedSizeOffset + oddOffset + cubeIdleLocation + perspectiveDepth;


                        Vector3 drawAnimPos = GetDrawPosFunction_Sphere(x, y, z);

                        Vector3 combinedPos = Vector3.Lerp(drawIdlePos, drawAnimPos, animationTime);

                        Graphics.DrawMesh(mesh, worldLeftBottomCorner + meshDepth + combinedPos, Quaternion.identity, mat, 0);


                        if (i > totalCubesCount) break;
                    }
                    if (i > totalCubesCount) break;
                }
                if (i > totalCubesCount) break;
            }

            //if (blueprintItem != null)
            //{
            //    Graphic materialGraphic = blueprint.prefab.graphic;

            //    Vector3 drawPos = drawLoc;
            //    drawPos.y += 0.046875f;

            //    drawPos.z += Mathf.Sin(Find.TickManager.TicksSinceSettle / 100f) * 0.1f;
            //    Rot4 rotation = Rot4.FromAngleFlat(Mathf.Sin(Find.TickManager.TicksSinceSettle / 20f));

            //    materialGraphic.Draw(drawPos, rotation, this);
            //}
        }

        private Vector3 GetDrawPosFunction_Cylinder(int x, int y, int z)
        {
            float anglePerCube = 360f / CUBES_AMOUNT_PER_SIDE;

            float rotateAnimationSpeed = 1 + 0.2f * Mathf.PerlinNoise(x + 0.5f, y + 0.5f) + 0.2f * Mathf.PerlinNoise(z + 0.5f, z + 0.5f);
            float animationAngle = 360f * (100 + Time.time) * rotateAnimationSpeed * 0.25f;

            Vector3 cubeAnimLocation = Quaternion.Euler(0, animationAngle + anglePerCube * x, 0) * Vector3.forward * (0.5f + z * 0.7f) + Vector3.up * y;
            cubeAnimLocation += new Vector3(0, 1.5f, 0);

            Vector3 cubeAnimDrawLocation = WorldToDrawPos(cubeAnimLocation);
            return def.size.ToVector3() / 2f + cubeAnimDrawLocation;
        }
        private Vector3 GetDrawPosFunction_Sphere(int x, int y, int z)
        {
            int index = x * CUBES_AMOUNT_PER_SIDE * CUBES_AMOUNT_PER_SIDE + y * CUBES_AMOUNT_PER_SIDE + z;

            float animationAngle = 360f * Time.time * 0.1f;
            Vector3 animPos = Quaternion.Euler(0, animationAngle, 0) * spherePoints[index] * 2f;
            animPos.z += 1.5f;


            //float distanceToFakeCamera = DistanceToFakeCamera(animPos);


            return def.size.ToVector3() / 2f + animPos;
        }

        //private float DistanceToFakeCamera(Vector3 worldPos)
        //{
        //    //Quaternion fakeCameraForward = Quaternion.LookRotation(new Vector3(0, FAKE_Y_COEF, 1).normalized);
        //    return 0;
        //}

        private void FillSherePointsArray(Vector3[] upts)
        {
            float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
            float off = 2.0f / upts.Length;
            float x = 0;
            float y = 0;
            float z = 0;
            float r = 0;
            float phi = 0;

            for (var k = 0; k < upts.Length; k++)
            {
                y = k * off - 1 + (off / 2);
                r = Mathf.Sqrt(1 - y * y);
                phi = k * inc;
                x = Mathf.Cos(phi) * r;
                z = Mathf.Sin(phi) * r;

                upts[k] = new Vector3(x, y, z);
            }
        }

        private Vector3 WorldToDrawPos(Vector3 worldPos)
        {
            return new Vector3(worldPos.x, 0, worldPos.z + FAKE_Y_COEF * worldPos.y);
        }

        public void CloneAndPlace(IntVec3 pos)
        {
            if (blueprint.prefab.category == ThingCategory.Item)
            {
                Thing item = ThingMaker.MakeThing(blueprint.prefab, blueprint.prefabStuff);
                GenPlace.TryPlaceThing(item, pos, Map, ThingPlaceMode.Direct);
            }
        }

        public void SetBlueprint(Thing item)
        {
            blueprintItem = CopyUtils.Copy(item);
            //blueprint =
        }

        public void OnBlueprintExtracted()
        {
            core?.AssignHolder(null);
        }

        public override string GetInspectString()
        {
            b.Clear();

            b.Append("Contains: ");
            b.Append(blueprintItem == null ? "nothing" : blueprintItem.Label);
            
            if (blueprintItem != null)
            {
                b.Append(" (");
                b.Append(blueprintItem.Label);
                b.Append(")");
            }

            return b.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (blueprintItem != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Extract blueprint",
                    icon = ContentFinder<Texture2D>.Get("temp3"),
                    action = () =>
                    {
                        //Thing item = ThingMaker.MakeThing(AstraDefOf.astra_blueprint);
                        //var comp = item.TryGetComp<ThingComp_AstraBlueprint>();
                        //comp.prefab = prefab;
                        //comp.prefabStuff = prefabStuff;
                        GenPlace.TryPlaceThing(blueprintItem, Position, Map, ThingPlaceMode.Near);

                        blueprintItem = null;
                        //prefabStuff = null;
                    }
                };
            }

            yield return new Command_Action
            {
                defaultLabel = "+0.1",
                icon = ContentFinder<Texture2D>.Get("temp3"),
                action = () =>
                {
                    animationTime = Mathf.Clamp01(animationTime + 0.1f);
                }
            };
            yield return new Command_Action
            {
                defaultLabel = "-0.1",
                icon = ContentFinder<Texture2D>.Get("temp3"),
                action = () =>
                {
                    animationTime = Mathf.Clamp01(animationTime - 0.1f);
                }
            };
        }
    }
}
