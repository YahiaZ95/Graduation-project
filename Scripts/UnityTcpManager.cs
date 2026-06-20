using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;


public enum CropType
{
    Trees,
    Palm
}


[Serializable]
public class Point2D
{
    public float x;
    public float z;
}

[Serializable]
public class Pipe
{
    public Point2D start;
    public Point2D end;
    public string pipe_type;
}

[Serializable]
public class AIFarmDebug
{
    public string crop_type;
    public float spacing;
    public string layout_mode;
    public float spacing_x;
    public float spacing_z;
    public string strategy;
    public string selected_strategy;
    public string hub_choice;
    public int tree_count;
    public int served_trees;
    public int unserved_trees;
    public float total_pipe_length;
    public int pipe_count;
    public int main_pipe_count;
    public int branch_pipe_count;
    public int lateral_pipe_count;
    public int forbidden_penalty;
    public float score;
    public float normalized_score;
    public bool reroute_used;
    public float well_safe_radius;
    public Point2D well_position;
}

[Serializable]
public class AIFarmResponse
{
    public List<TreePoint> trees;
    public List<Pipe> pipes;
    public AIFarmDebug debug;
}

[Serializable]
public class ForbiddenZone
{
    public float x;
    public float z;
    public float radius;
}

[Serializable]
public class TestPreset
{
    public string name;
    public float width;
    public float height;
    public Vector2 well_position;
    public CropType crop_type;
    public float well_safe_radius;
    public List<ForbiddenZone> forbidden_zones;
}

[Serializable]
public class FarmData
{
    public float width;
    public float height;
    public Point2D well_position;
    public CropType crop_type;
    public float well_safe_radius;
    public List<ForbiddenZone> forbidden_zones;
}

[Serializable]
public class TreePoint
{
    public float x;
    public float z;
}

public class UnityTcpManager : MonoBehaviour
{
    public float wellSafeRadius = 2.0f;

    [Header("References")]
    public PlantationLayout plantation;
    public IrrigationSystem irrigation;
    public GroundGenerator ground;
    public DebugUIController debugUI;
    public ConstraintVisualizer constraintVisualizer;

    [Header("Backend")]
    [SerializeField] private string backendHost = "127.0.0.1";
    [SerializeField] private int backendPort = 5005;
    [SerializeField] private int connectTimeoutMs = 3000;
    [SerializeField] private int backendStartupTimeoutMs = 10000;
    [SerializeField] private bool autoStartBackend = true;
    [SerializeField] private bool stopBackendOnQuit = true;

    [Header("Test Presets")]
    public List<TestPreset> testPresets;

    private Process _backendProcess;
    private string _backendRootPath;

    void Start()
    {
        if (debugUI == null) debugUI = FindObjectOfType<DebugUIController>();
        if (constraintVisualizer == null) constraintVisualizer = FindObjectOfType<ConstraintVisualizer>();
        _backendRootPath = Path.Combine(Application.streamingAssetsPath, "AI");
    }

    void OnApplicationQuit()
    {
        if (!stopBackendOnQuit)
            return;

        try
        {
            if (_backendProcess != null && !_backendProcess.HasExited)
                _backendProcess.Kill();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("[UnityTcpManager] Failed to stop backend on quit: " + e.Message);
        }
    }

    public IEnumerator SendFarmDataCoroutine(float width, float length, Vector3 wellPos, CropType cropType, float wellSafeRadius, List<ForbiddenZone> forbiddenZones = null)
    {
        FarmData data = new FarmData
        {
            width = width,
            height = length,
            well_position = new Point2D { x = wellPos.x, z = wellPos.z },
            crop_type = cropType,
            well_safe_radius = wellSafeRadius,
            forbidden_zones = forbiddenZones ?? new List<ForbiddenZone>()
        };

        string jsonInput = JsonUtility.ToJson(data);

        if (!IsBackendReachable())
        {
            if (!autoStartBackend)
            {
                UnityEngine.Debug.LogError("[UnityTcpManager] AI backend is not reachable and auto-start is disabled.");
                yield break;
            }

            if (!StartBackendProcess())
            {
                UnityEngine.Debug.LogError("[UnityTcpManager] Failed to start AI backend. Check StreamingAssets/AI deployment files.");
                yield break;
            }

            float waitUntil = Time.realtimeSinceStartup + (backendStartupTimeoutMs / 1000f);
            while (Time.realtimeSinceStartup < waitUntil)
            {
                if (IsBackendReachable())
                    break;
                yield return null;
            }

            if (!IsBackendReachable())
            {
                UnityEngine.Debug.LogError("[UnityTcpManager] AI backend did not become ready within timeout.");
                yield break;
            }
        }

        string output;
        try
        {
            output = SendRequestToBackend(jsonInput);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[UnityTcpManager] Backend request failed: " + e.Message);
            yield break;
        }

        UnityEngine.Debug.Log("AI Backend Output: " + output);

        try
        {
            var aiResponse = JsonUtility.FromJson<AIFarmResponse>(output.Trim());

            if (aiResponse.trees != null)
                plantation.BuildFromAI(aiResponse.trees);

            if (aiResponse.pipes != null)
                irrigation.BuildFromAI(aiResponse.pipes);

            if (debugUI != null && aiResponse.debug != null)
                debugUI.SetDebug(aiResponse.debug);

            if (constraintVisualizer != null)
            {
                constraintVisualizer.SetConstraints(data.forbidden_zones, new Vector3(data.well_position.x, 0f, data.well_position.z), data.well_safe_radius);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[UnityTcpManager] JSON parse error: " + e.Message);
        }
    }

    public void SendFarmDataFromGround(CropType cropType)
    {
    }

    public IEnumerator SendFarmDataFromGroundCoroutine(CropType cropType)
    {
        if (ground == null)
        {
            UnityEngine.Debug.LogWarning("Ground generator is not assigned.");
            yield break;
        }

        yield return StartCoroutine(SendFarmDataCoroutine(ground.width, ground.length, ground.well_pos, cropType, wellSafeRadius));
    }

    public void ApplyPreset(int index)
    {
        if (testPresets == null || index < 0 || index >= testPresets.Count)
        {
            UnityEngine.Debug.LogWarning("Invalid preset index.");
            return;
        }

        var preset = testPresets[index];

        if (ground != null)
        {
            ground.width = preset.width;
            ground.length = preset.height;
            ground.well_pos = new Vector3(preset.well_position.x, 0f, preset.well_position.y);
            ground.GenerateGround();
            ground.SetWell(ground.well_pos);
        }

        wellSafeRadius = preset.well_safe_radius;

        StartCoroutine(SendFarmDataCoroutine(
            preset.width,
            preset.height,
            new Vector3(preset.well_position.x, 0f, preset.well_position.y),
            preset.crop_type,
            wellSafeRadius,
            preset.forbidden_zones));
    }

    private bool StartBackendProcess()
    {
        if (_backendProcess != null)
        {
            try
            {
                if (!_backendProcess.HasExited)
                {
                    UnityEngine.Debug.Log("[UnityTcpManager] AI backend process already running.");
                    return true;
                }
            }
            catch
            {
            }
        }

        ProcessStartInfo startInfo = null;

        foreach (string backendRoot in GetBackendSearchRoots())
        {
            if (string.IsNullOrWhiteSpace(backendRoot))
                continue;

            string backendExePath = Path.Combine(backendRoot, "AIBackend.exe");
            string launcherBatPath = Path.Combine(backendRoot, "run_backend.bat");
            string launcherCmdPath = Path.Combine(backendRoot, "run_backend.cmd");
            string serverPyPath = Path.Combine(backendRoot, "server.py");

            if (File.Exists(backendExePath))
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = backendExePath,
                    WorkingDirectory = backendRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                UnityEngine.Debug.Log("[UnityTcpManager] Starting AI backend executable: " + backendExePath);
                break;
            }

            if (File.Exists(launcherBatPath) || File.Exists(launcherCmdPath))
            {
                string launcherPath = File.Exists(launcherBatPath) ? launcherBatPath : launcherCmdPath;
                startInfo = new ProcessStartInfo
                {
                    FileName = launcherPath,
                    WorkingDirectory = backendRoot,
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                UnityEngine.Debug.Log("[UnityTcpManager] Starting AI backend launcher: " + launcherPath);
                break;
            }

            if (File.Exists(serverPyPath))
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{serverPyPath}\" --server --host {backendHost} --port {backendPort}",
                    WorkingDirectory = backendRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                UnityEngine.Debug.Log("[UnityTcpManager] Starting AI backend via python fallback: " + serverPyPath);
                break;
            }
        }

        if (startInfo == null)
        {
            UnityEngine.Debug.LogError("[UnityTcpManager] No AI backend launch target found. Checked: " + string.Join(" | ", GetBackendSearchRoots()));
            return false;
        }

        try
        {
            _backendProcess = Process.Start(startInfo);
            if (_backendProcess != null && !startInfo.UseShellExecute)
            {
                _backendProcess.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        UnityEngine.Debug.Log("[AIBackend] " + e.Data);
                };
                _backendProcess.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        UnityEngine.Debug.LogError("[AIBackend] " + e.Data);
                };
                _backendProcess.BeginOutputReadLine();
                _backendProcess.BeginErrorReadLine();
            }
            return true;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[UnityTcpManager] Failed to launch AI backend: " + e.Message);
            return false;
        }
    }

    private string[] GetBackendSearchRoots()
    {
        string gameRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        return new[]
        {
            _backendRootPath,
            Path.Combine(gameRoot, "AI"),
            gameRoot
        };
    }

    private bool IsBackendReachable()
    {
        try
        {
            using (var client = new TcpClient())
            {
                var result = client.BeginConnect(backendHost, backendPort, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(connectTimeoutMs);
                if (!success)
                    return false;

                client.EndConnect(result);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private string SendRequestToBackend(string jsonInput)
    {
        using (var client = new TcpClient())
        {
            var result = client.BeginConnect(backendHost, backendPort, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(connectTimeoutMs);
            if (!success)
                throw new TimeoutException($"Timed out connecting to AI backend at {backendHost}:{backendPort}");

            client.EndConnect(result);
            client.ReceiveTimeout = connectTimeoutMs;
            client.SendTimeout = connectTimeoutMs;

            using (NetworkStream stream = client.GetStream())
            {
                byte[] requestBytes = Encoding.UTF8.GetBytes(jsonInput + "\n");
                stream.Write(requestBytes, 0, requestBytes.Length);
                stream.Flush();

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string response = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(response))
                        throw new IOException("AI backend returned an empty response.");
                    return response;
                }
            }
        }
    }
}
