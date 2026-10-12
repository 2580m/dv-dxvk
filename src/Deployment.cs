using System;
using System.IO;
using System.Security.Cryptography;

namespace DVVulkan
{
    public static class Deployment
    {
        public static readonly string[] Names = { "d3d11.dll", "dxgi.dll" };
        public static readonly string[] Hashes = {
            "0A203B6255C893430F0A8461C618962DF04E62AA41538D5A875B21868317CF9F",
            "E3C1178EB7F0DD59F91BCFCC257549E5B9CFDB5E8508719293DE7F8704F53160"
        };

        public static bool Matches(string path, int index)
        {
            if (!File.Exists(path)) return false;
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "") == Hashes[index];
        }

        public static void Validate(string root, string mod)
        {
            if (!File.Exists(Path.Combine(root, "DerailValley.exe")))
                throw new ModException("NotGame");
            for (int i = 0; i < Names.Length; i++)
            {
                if (!Matches(Path.Combine(mod, "dxvk", Names[i]), i))
                    throw new ModException("PayloadCorrupt", Names[i]);
                string destination = Path.Combine(root, Names[i]);
                if (File.Exists(destination) && !Matches(destination, i))
                    throw new ModException("ForeignDll", destination);
            }
        }

        public static void Apply(string root, string mod, bool enable)
        {
            Validate(root, mod);
            bool[] existed = new bool[Names.Length];
            for (int i = 0; i < Names.Length; i++) existed[i] = File.Exists(Path.Combine(root, Names[i]));
            try
            {
                for (int i = 0; i < Names.Length; i++)
                {
                    string destination = Path.Combine(root, Names[i]);
                    if (enable && !existed[i]) File.Copy(Path.Combine(mod, "dxvk", Names[i]), destination, false);
                    if (!enable && existed[i]) File.Delete(destination);
                }
            }
            catch
            {
                for (int i = 0; i < Names.Length; i++)
                {
                    string destination = Path.Combine(root, Names[i]);
                    if (!existed[i] && Matches(destination, i)) File.Delete(destination);
                    if (existed[i] && !File.Exists(destination)) File.Copy(Path.Combine(mod, "dxvk", Names[i]), destination, false);
                }
                throw;
            }
        }
    }
}
