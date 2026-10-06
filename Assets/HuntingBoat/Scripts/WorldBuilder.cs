using System.Collections.Generic;
using UnityEngine;

namespace HuntingBoat
{
    public sealed class OceanWorld
    {
        public Transform Root, BoatVisual, Rod, Fish;
        public OceanBoat Boat;
        public Light Sun;
        public Material Water;
        public ParticleSystem Wake;
        public readonly Vector3[] Spots = { new Vector3(65, 0, 90), new Vector3(-240, 0, 130), new Vector3(190, 0, -235) };
        public readonly Vector3[] Islands = { new Vector3(-85, 0, 170), new Vector3(220, 0, 145), new Vector3(-220, 0, -195) };
        public readonly string[] SpotNames = { "Lighthouse Shoal", "Bluewater Banks", "South Reef" };
    }

    public static class WorldBuilder
    {
        private static Material Material(string name, Color color, float metallic = 0, float gloss = .3f)
        {
            var m = new Material(Shader.Find("HuntingBoat/Lit")); m.name = name; m.color = color;
            m.SetFloat("_Metallic", metallic); m.SetFloat("_Glossiness", gloss); return m;
        }
        public static GameObject Shape(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool collider = false)
        {
            var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(parent, false);
            g.transform.localPosition = position; g.transform.localScale = scale; g.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) Object.Destroy(g.GetComponent<Collider>()); return g;
        }
        private static void Beam(Transform parent, string name, Vector3 a, Vector3 b, float radius, Material material)
        {
            var g = Shape(PrimitiveType.Cylinder, parent, name, (a + b) * .5f, new Vector3(radius * 2, Vector3.Distance(a, b) * .5f, radius * 2), material);
            g.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }
        public static OceanWorld Build()
        {
            var world = new OceanWorld(); world.Root = new GameObject("Procedural archipelago").transform;
            var white = Material("Ivory hull", new Color(.83f, .86f, .84f), .12f, .7f);
            var navy = Material("Deep navy", new Color(.022f, .075f, .11f), .15f, .65f);
            var metal = Material("Rail chrome", new Color(.65f, .72f, .76f), .85f, .8f);
            var wood = Material("Teak deck", new Color(.37f, .22f, .10f), 0, .25f);
            var rock = Material("Cliff stone", new Color(.32f, .34f, .29f));
            var green = Material("Island foliage", new Color(.13f, .25f, .10f));
            var sand = Material("Warm shore", new Color(.64f, .58f, .38f));
            var orange = Material("Safety orange", new Color(.98f, .25f, .055f));

            var water = new GameObject("Animated ocean"); water.transform.SetParent(world.Root);
            water.AddComponent<MeshFilter>().sharedMesh = OceanMesh(160, 2000);
            world.Water = new Material(Shader.Find("HuntingBoat/Ocean"));
            water.AddComponent<MeshRenderer>().sharedMaterial = world.Water;
            water.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            water.GetComponent<Renderer>().receiveShadows = false;

            var boat = new GameObject("Explorer 550"); boat.transform.SetParent(world.Root); boat.transform.position = new Vector3(0, .5f, 0);
            world.Boat = boat.AddComponent<OceanBoat>();
            world.BoatVisual = new GameObject("Boat visual sway").transform; world.BoatVisual.SetParent(boat.transform, false); world.Boat.Visual = world.BoatVisual;
            Hull(world.BoatVisual, white, navy);
            Shape(PrimitiveType.Cube, world.BoatVisual, "Teak cockpit deck", new Vector3(0, .65f, -.6f), new Vector3(2.55f, .16f, 4.7f), wood);
            Shape(PrimitiveType.Cube, world.BoatVisual, "Center console", new Vector3(0, 1.25f, .3f), new Vector3(1.15f, 1.1f, 1.15f), white);
            var windshield = Shape(PrimitiveType.Cube, world.BoatVisual, "Tinted windshield", new Vector3(0, 2f, .75f), new Vector3(1.2f, .6f, .08f), navy);
            windshield.transform.localRotation = Quaternion.Euler(-15, 0, 0);
            Shape(PrimitiveType.Cube, world.BoatVisual, "Helm display", new Vector3(0, 1.72f, -.3f), new Vector3(.65f, .24f, .06f), navy);
            for (int side = -1; side <= 1; side += 2)
            {
                Beam(world.BoatVisual, "Canopy support", new Vector3(side * .8f, .7f, -.8f), new Vector3(side * .8f, 2.7f, -.8f), .035f, metal);
                Beam(world.BoatVisual, "Canopy forward support", new Vector3(side * .8f, .7f, 1), new Vector3(side * .8f, 2.7f, 1), .035f, metal);
                Beam(world.BoatVisual, "Side rail", new Vector3(side * 1.35f, 1, -2.7f), new Vector3(side * 1.15f, 1, 2.6f), .035f, metal);
                for (int i = 0; i < 4; i++) Beam(world.BoatVisual, "Rail stanchion", new Vector3(side * 1.3f, .55f, -2.3f + i * 1.2f), new Vector3(side * 1.3f, 1, -2.3f + i * 1.2f), .026f, metal);
                Shape(PrimitiveType.Cube, world.BoatVisual, "Outboard housing", new Vector3(side * .7f, .35f, -3.65f), new Vector3(.65f, 1.1f, .6f), navy);
                Shape(PrimitiveType.Cube, world.BoatVisual, "Outboard silver cap", new Vector3(side * .7f, .9f, -3.65f), new Vector3(.68f, .3f, .65f), metal);
                Beam(world.BoatVisual, "Transom rod", new Vector3(side * 1.1f, .8f, -2.7f), new Vector3(side * 1.35f, 3.6f, -3.3f), .015f, navy);
            }
            Shape(PrimitiveType.Cube, world.BoatVisual, "Canopy roof", new Vector3(0, 2.8f, .1f), new Vector3(2.2f, .16f, 2.75f), white);
            Shape(PrimitiveType.Cube, world.BoatVisual, "Helm seat", new Vector3(0, 1, -1.3f), new Vector3(1.1f, .45f, .6f), white);
            Shape(PrimitiveType.Cube, world.BoatVisual, "Seat back", new Vector3(0, 1.35f, -1.55f), new Vector3(1.1f, .6f, .13f), navy);
            var ring = Shape(PrimitiveType.Cylinder, world.BoatVisual, "Safety float", new Vector3(1.45f, 1, -.9f), new Vector3(.58f, .10f, .58f), orange);
            ring.transform.localRotation = Quaternion.Euler(0, 0, 90);
            world.Rod = new GameObject("Fishing rod").transform; world.Rod.SetParent(world.BoatVisual, false); world.Rod.localPosition = new Vector3(1, .9f, 1.1f);
            Beam(world.Rod, "Rod grip", Vector3.zero, new Vector3(.08f, .5f, .18f), .04f, wood);
            Beam(world.Rod, "Rod lower", new Vector3(.08f, .5f, .18f), new Vector3(.17f, 1.4f, .75f), .02f, navy);
            Beam(world.Rod, "Rod tip", new Vector3(.17f, 1.4f, .75f), new Vector3(.15f, 1.9f, 1.6f), .012f, navy);
            Shape(PrimitiveType.Cylinder, world.Rod, "Reel", new Vector3(.18f, .35f, .1f), new Vector3(.18f, .10f, .18f), metal);
            var line = world.Rod.gameObject.AddComponent<LineRenderer>(); line.positionCount = 2; line.startWidth = .012f; line.endWidth = .007f; line.sharedMaterial = Material("Fishing line", new Color(.58f, .88f, .92f)); line.enabled = false;
            var fishMat = Material("Tuna silver", new Color(.35f, .55f, .64f), .7f, .8f);
            world.Fish = FishModel(world.Root, fishMat, Material("Yellow fins", new Color(.92f, .67f, .08f)));
            world.Fish.gameObject.SetActive(false);
            world.Wake = Wake(boat.transform);

            var rng = new System.Random(451);
            for (int island = 0; island < world.Islands.Length; island++)
            {
                Vector3 position = world.Islands[island];
                var land = new GameObject("Island " + island).transform; land.SetParent(world.Root); land.position = position;
                Shape(PrimitiveType.Sphere, land, "Shore shelf", new Vector3(0, -9, 0), new Vector3(130, 22, 115), sand);
                Shape(PrimitiveType.Sphere, land, "Main island", new Vector3(0, 3, 0), new Vector3(92, 45, 83), rock);
                var collision = land.gameObject.AddComponent<SphereCollider>(); collision.center = new Vector3(0, 0, 0); collision.radius = 58;
                for (int j = 0; j < 25; j++)
                {
                    float x = (float)(rng.NextDouble() - .5) * 75, z = (float)(rng.NextDouble() - .5) * 70;
                    float y = 6 + (float)rng.NextDouble() * 20;
                    Shape(PrimitiveType.Sphere, land, "Cliff outcrop", new Vector3(x, y - 5, z), new Vector3(12 + j % 4 * 3, y, 11), rock);
                    Shape(PrimitiveType.Sphere, land, "Shrub canopy", new Vector3(x, y + 2, z), new Vector3(11, 5, 11), green);
                }
                Lighthouse(land, white, orange, navy);
            }
            for (int i = 0; i < world.Spots.Length; i++)
            {
                var buoy = Shape(PrimitiveType.Cylinder, world.Root, world.SpotNames[i] + " buoy", world.Spots[i] + Vector3.up * .6f, new Vector3(.7f, .6f, .7f), orange);
                Beam(buoy.transform, "Buoy mast", Vector3.zero, Vector3.up * 2, .07f, white);
                Shape(PrimitiveType.Sphere, buoy.transform, "Buoy beacon", Vector3.up * 2, Vector3.one * .3f, orange);
            }
            var sun = new GameObject("Sun"); world.Sun = sun.AddComponent<Light>(); world.Sun.type = LightType.Directional; world.Sun.shadows = LightShadows.Soft; world.Sun.shadowStrength = .7f;
            var sky = new Material(Shader.Find("HuntingBoat/Sky")); sky.SetFloat("_Exposure", 1.1f); RenderSettings.skybox = sky;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .0018f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            return world;
        }
        private static void Hull(Transform parent, Material white, Material navy)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            // Five longitudinal hull rings, each with port/starboard gunwale and keel.
            float[] z = { -3.5f, -2.5f, 0, 2.2f, 3.9f }; float[] width = { 1.45f, 1.6f, 1.6f, 1.12f, .06f };
            for (int i = 0; i < z.Length; i++)
            {
                vertices.Add(new Vector3(-width[i], .7f, z[i])); vertices.Add(new Vector3(width[i], .7f, z[i]));
                vertices.Add(new Vector3(-width[i] * .65f, -.35f, z[i])); vertices.Add(new Vector3(width[i] * .65f, -.35f, z[i]));
            }
            int[] order = { 0, 1, 3, 2 };
            for (int ring = 0; ring < 4; ring++) for (int edge = 0; edge < 4; edge++)
            {
                int a = ring * 4 + order[edge], b = ring * 4 + order[(edge + 1) % 4], c = a + 4, d = b + 4;
                triangles.AddRange(new[] { a, c, b, b, c, d });
            }
            triangles.AddRange(new[] { 0, 1, 2, 1, 3, 2, 16, 18, 17, 17, 18, 19 });
            var mesh = new Mesh { name = "Original Explorer hull", vertices = vertices.ToArray(), triangles = triangles.ToArray() }; mesh.RecalculateNormals();
            var hull = new GameObject("Sculpted hull"); hull.transform.SetParent(parent, false); hull.AddComponent<MeshFilter>().sharedMesh = mesh; hull.AddComponent<MeshRenderer>().sharedMaterial = white;
            Shape(PrimitiveType.Cube, parent, "Navy transom stripe", new Vector3(0, .15f, -3.51f), new Vector3(2.9f, .25f, .04f), navy);
        }
        private static Mesh OceanMesh(int divisions, float size)
        {
            var vertices = new Vector3[(divisions + 1) * (divisions + 1)]; var uv = new Vector2[vertices.Length]; var triangles = new int[divisions * divisions * 6]; int n = 0;
            for (int z = 0; z <= divisions; z++) for (int x = 0; x <= divisions; x++)
            {
                int i = z * (divisions + 1) + x; vertices[i] = new Vector3((x / (float)divisions - .5f) * size, 0, (z / (float)divisions - .5f) * size); uv[i] = new Vector2(x / (float)divisions, z / (float)divisions);
                if (x < divisions && z < divisions) { triangles[n++] = i; triangles[n++] = i + divisions + 1; triangles[n++] = i + 1; triangles[n++] = i + 1; triangles[n++] = i + divisions + 1; triangles[n++] = i + divisions + 2; }
            }
            var mesh = new Mesh { name = "Ocean grid", vertices = vertices, uv = uv, triangles = triangles }; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            // Vertex displacement can move the mesh beyond its initial flat bounds.
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(size, 30, size)); return mesh;
        }
        private static void Lighthouse(Transform land, Material white, Material orange, Material navy)
        {
            var root = new GameObject("Lighthouse").transform; root.SetParent(land, false); root.localPosition = new Vector3(12, 24, 5);
            Shape(PrimitiveType.Cylinder, root, "Tower base", Vector3.zero, new Vector3(8, 2, 8), navy);
            Shape(PrimitiveType.Cylinder, root, "Ivory tower", Vector3.up * 13, new Vector3(5, 12, 5), white);
            Shape(PrimitiveType.Cylinder, root, "Tower stripe", Vector3.up * 16, new Vector3(5.1f, 2, 5.1f), orange);
            Shape(PrimitiveType.Cylinder, root, "Lantern house", Vector3.up * 27, new Vector3(6, 2, 6), navy);
            Shape(PrimitiveType.Sphere, root, "Lantern glow", Vector3.up * 27, new Vector3(4.2f, 1.5f, 4.2f), orange);
            var light = new GameObject("Navigation beacon").AddComponent<Light>(); light.transform.SetParent(root, false); light.transform.localPosition = Vector3.up * 27; light.type = LightType.Point; light.color = new Color(1, .75f, .35f); light.range = 80; light.intensity = 3;
        }
        private static Transform FishModel(Transform parent, Material body, Material fin)
        {
            var fish = new GameObject("Hooked fish").transform; fish.SetParent(parent, false);
            Shape(PrimitiveType.Sphere, fish, "Streamlined body", Vector3.zero, new Vector3(.35f, .5f, 1.5f), body);
            Shape(PrimitiveType.Sphere, fish, "Head", new Vector3(0, 0, .62f), new Vector3(.30f, .36f, .5f), body);
            var tail = Shape(PrimitiveType.Cube, fish, "Forked tail", new Vector3(0, 0, -.87f), new Vector3(.06f, .65f, .24f), fin); tail.transform.localRotation = Quaternion.Euler(0, 0, 35);
            Shape(PrimitiveType.Cube, fish, "Dorsal fin", new Vector3(0, .28f, -.1f), new Vector3(.03f, .3f, .35f), fin);
            Shape(PrimitiveType.Sphere, fish, "Eye", new Vector3(.155f, .09f, .73f), Vector3.one * .065f, Material("Fish eye", Color.black)); return fish;
        }
        private static ParticleSystem Wake(Transform boat)
        {
            var g = new GameObject("Wake spray"); g.transform.SetParent(boat, false); g.transform.localPosition = new Vector3(0, -.25f, -3.8f); g.transform.localRotation = Quaternion.Euler(75, 0, 0);
            var p = g.AddComponent<ParticleSystem>(); var main = p.main; main.startLifetime = 2; main.startSpeed = 2; main.startSize = .45f; main.startColor = new Color(.75f, .94f, 1, .5f); main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 250;
            var emission = p.emission; emission.rateOverTime = 0;
            var shape = p.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 28; shape.radius = 1;
            var renderer = p.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = new Material(Shader.Find("HuntingBoat/Wake")); return p;
        }
    }
}
