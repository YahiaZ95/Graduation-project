"""TCP entry point for the farm layout AI backend."""

import json
import os
import socket
import sys

# Ensure sibling modules resolve when launched as a script.
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from evaluate import process_request


def start_server(host="127.0.0.1", port=5005):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind((host, port))
    server_socket.listen(1)
    print(f"AI server listening at {host}:{port}")

    while True:
        conn, addr = server_socket.accept()
        print(f"Connection received from {addr}")
        buffer = ""

        while True:
            data = conn.recv(4096)
            if not data:
                break

            buffer += data.decode("utf-8")
            while "\n" in buffer:
                line, buffer = buffer.split("\n", 1)
                if not line.strip():
                    continue

                try:
                    json_data = json.loads(line)
                except json.JSONDecodeError as e:
                    print("JSON decode error:", e)
                    continue

                print("Received:", json_data)
                response = process_request(json_data)
                conn.send((json.dumps(response) + "\n").encode())
                print("Sent AI layout.")


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--server":
        host = "127.0.0.1"
        port = 5005
        if "--host" in sys.argv:
            host_index = sys.argv.index("--host")
            if host_index + 1 < len(sys.argv):
                host = sys.argv[host_index + 1]
        if "--port" in sys.argv:
            port_index = sys.argv.index("--port")
            if port_index + 1 < len(sys.argv):
                port = int(sys.argv[port_index + 1])
        start_server(host=host, port=port)
    elif len(sys.argv) > 1:
        json_str = sys.argv[1]
        json_data = json.loads(json_str)
        response = process_request(json_data)
        print(json.dumps(response))
    else:
        try:
            json_str = sys.stdin.read().strip()
            if json_str:
                json_data = json.loads(json_str)
                response = process_request(json_data)
                print(json.dumps(response))
            else:
                start_server()
        except Exception:
            start_server()
