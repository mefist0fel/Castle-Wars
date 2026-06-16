using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace CastleWars.Editor
{
    public static class MessagePackCodeGen
    {
        // Source files that contain MessagePack-annotated network types.
        // mpu.exe uses the legacy C# 5 compiler (CSC), so these files must stay C# 5-compatible:
        //   - no #nullable enable
        //   - no auto-property initializers (= new List<>() / = string.Empty)
        //   - use constructors for initialization instead
        // NetCommand.cs is a stub now — only EntitySnapshot and NetBatches contain live types.
        private static readonly string[] NetworkSources =
        {
            Path.Combine("Assets", "SharedCode", "Network", "EntitySnapshot.cs"),
            Path.Combine("Assets", "SharedCode", "Network", "NetBatches.cs"),
        };

        // Only concrete (non-abstract) types — mpu.exe cannot generate serializers
        // for abstract base classes even if they appear in polymorphic collections.
        // The runtime handles abstract-type dispatch; we only need AOT code for concrete types.
        private const string IncludesPattern =
            "(CommandBatch|EntityUpdateBatch" +
            "|TeleportArmyNetCommand|AttackArmyNetCommand|CaptureCityNetCommand" +
            "|HealArmyNetCommand|CreateGameNetCommand" +
            "|SessionSnapshot|PlayerSnapshot|MapSnapshot" +
            "|RegionSnapshot|ArmySnapshot|CitySnapshot)";

        [MenuItem("Tools/MessagePack/Regenerate Serializers")]
        private static void RegenerateSerializers()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath)!;
            var mpuPath     = Path.Combine(projectRoot, "Assets", "Plugin", "MessagePack", "mpu", "net45", "mpu.exe");
            var outPath     = Path.Combine(projectRoot, "Assets", "SharedCode", "Generated");

            if (!File.Exists(mpuPath))
            {
                EditorUtility.DisplayDialog("MessagePack Code Gen", $"mpu.exe not found:\n{mpuPath}", "OK");
                return;
            }

            Directory.CreateDirectory(outPath);

            // Build absolute source paths and quote each one (paths may contain spaces).
            var sourceArgs = string.Join(" ", System.Array.ConvertAll(NetworkSources,
                s => $"\"{Path.Combine(projectRoot, s)}\""));

            // -s              = generate serializer source code
            // --singular      = don't recurse into referenced types (avoids abstract-type error)
            // --includes      = regex filter — only concrete network types
            // -o              = output directory
            // (no -r)         = mpu.exe auto-finds ./MsgPack.dll in its own directory
            var args = $"-s --singular --includes \"{IncludesPattern}\" -o \"{outPath}\" {sourceArgs}";

            EditorUtility.DisplayProgressBar("MessagePack", "Running mpu.exe ...", 0.4f);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName               = mpuPath,
                    Arguments              = args,
                    WorkingDirectory       = Path.GetDirectoryName(mpuPath),
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true,
                };

                using var proc   = Process.Start(psi)!;
                var stdout = proc.StandardOutput.ReadToEnd();
                var stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                // mpu.exe: 0 = success, 1 = help shown (bad args), 2+ = error.
                // All messages go to stdout (not stderr).
                if (proc.ExitCode == 0)
                {
                    UnityEngine.Debug.Log($"[MsgPack CodeGen] Done — {outPath}\n{stdout}".TrimEnd());
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("MessagePack Code Gen", "Serializers regenerated.", "OK");
                }
                else
                {
                    var detail = string.IsNullOrWhiteSpace(stdout) ? "(no output)" : stdout.Trim();
                    UnityEngine.Debug.LogError($"[MsgPack CodeGen] mpu.exe exited {proc.ExitCode}:\n{detail}");
                    EditorUtility.DisplayDialog("MessagePack Code Gen",
                        $"mpu.exe exited {proc.ExitCode}.\nSee Console for details.\n\n{detail}", "OK");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MsgPack CodeGen] {ex}");
                EditorUtility.DisplayDialog("MessagePack Code Gen", $"Exception: {ex.Message}", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
