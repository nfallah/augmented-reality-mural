# Augmented Reality (AR) Mural

This GitHub repository is a mirror of two separate GitLab repositories. The source code can be found at the following links:
[Client Code](https://gitlab.orbit-lab.org/nfallah/ar-mural)
[Server Code](https://gitlab.orbit-lab.org/nfallah/ar-mural-server)

This project encapsulates a collaborative augmented reality (AR) application that enables remote users to create a virtual mural together. The client-side architecture is built using the Unity game engine and can be deployed on the Microsoft HoloLens 2. Running the Unity project on a computer also allows you to connect to the server.

The server-side architecture is also developed in C#/.NET, with a C++ to C# wrapper for LevelDB, which stores a database of drawings to enable multi-session persistence for all players. To ensure low latency, the project utilizes the Message Queuing Telemetry Transport (MQTT) protocol.

To learn more about this project, including video footage and a wiki documenting weekly progress, please visit [this page](https://www.orbit-lab.org/wiki/Other/Summer/2024/AR_Mural).
