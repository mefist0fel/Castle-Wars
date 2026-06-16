using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace CastleWars.Editor
{
    public static class MessagePackCodeGen
    {
        [MenuItem("Tools/MessagePack/Regenerate Serializers")]
        private static void RegenerateSerializers()
        {
            var projectRoot  = Path.GetDirectoryName(Application.dataPath)!;
            var mpuPath      = Path.Combine(projectRoot, "Assets", "Plugin", "MessagePack", "mpu", "net45", "mpu.exe");
            var dllPath      = Path.Combine(projectRoot, "Library", "ScriptAssemblies", "CastleWars.Shared.dll");
            var msgpackDll   = Path.Combine(projectRoot, "Assets", "Plugin", "MessagePack", "MsgPack.dll");
            var outPath      = Path.Combine(projectRoot, "Assets", "SharedCode", "Generated");

            if (!File.Exists(mpuPath))
            {
                EditorUtility.DisplayDialog("MessagePack Code Gen", $"mpu.exe not found:\n{mpuPath}", "OK");
                return;
            }
            if (!File.Exists(dllPath))
            {
                EditorUtility.DisplayDialog("MessagePack Code Gen",
                    "CastleWars.Shared.dll not found — make sure the project compiles first.\n\n" + dllPath, "OK");
                return;
            }

            Directory.CreateDirectory(outPath);
            EditorUtility.DisplayProgressBar("MessagePack", "Running mpu.exe ...", 0.4f);

            try
            {
                // Correct mpu.exe syntax (from --help):
                //   mpu -s -a <assembly> -o <outputDir> [-r <refDll,...>]
                // -s = generate serializer sources
                // -a = input is a compiled assembly (not C# source)
                // -r = additional reference assemblies (MsgPack.dll is auto-added if present
                //      in the current dir, but we pass it explicitly to be safe)
                var args = $"-s -a \"{dllPath}\" -o \"{outPath}\" -r \"{msgpackDll}\"";

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

                // mpu.exe exit codes: 0 = success, 1 = help shown (bad args), 2 = invalid args
                // Errors and help both go to STDOUT (not stderr).
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
