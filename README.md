# My Portfolio | SetPlayList

## Table of Contents
- [Project Description](#project-description)
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Unit Tests](#unit-tests)
- [Live Version](#live-version)
- [Important Note Regarding Spotify API](#important-note-regarding-spotify-api)
- [Screenshots](#screenshots)
- [How to Run Locally](#how-to-run-locally)

## Project Description
This project serves as my personal portfolio, showcasing my skills in building full-stack web applications. The core of the project is an application that integrates data from the Spotify API and Setlist.fm API, automating the process of creating Spotify playlists based on saved concert setlists. The application also offers real-time playlist editing capabilities.

The entire project is built using the ASP.NET platform in .NET 8, with the frontend layer implemented in Blazor technology. The user interface has been designed using the Bootstrap framework, ensuring responsiveness and an aesthetic appearance.

## Features
*   **Setlist.fm Integration**: Fetches setlists from concerts that have been saved on the Setlist.fm platform.
*   **Spotify Integration**: Automatically creates Spotify playlists containing tracks from the fetched setlists.
*   **Real-time Editing**: Ability to preview and modify the Spotify playlist before its final saving.
*   **Intuitive User Interface**: Thanks to Blazor and Bootstrap, the application is easy to use and visually appealing.
*   **Portfolio**: The project serves as a demonstration of my full-stack development skills.

## Technologies Used
*   **Backend**: ASP.NET Core (.NET 8)
*   **Frontend**: Blazor
*   **Styling**: Bootstrap
*   **API Integrations**:
    *   Spotify API
    *   Setlist.fm API

## Unit Tests
The project includes a suite of unit tests that ensure the reliability and correct functioning of key business logic components and API integrations.

## Live Version
The application is available online at:
[https://zlotekmprofile.net/](https://zlotekmprofile-fff0ghczcfa3dyfs.polandcentral-01.azurewebsites.net/)

## Important Note Regarding Spotify API
Unfortunately due to latest Spotify policy changes, only companies with at least 250 000 monthly active users can use Spotify API to create playlists on behalf of users.
If you want to use SetPlayList on my website please contact me via email.

## Screenshots
Below are screenshots showcasing key aspects of the application, including its user interface and the Spotify integration in action:

![Screenshot 1](SetPlayList.Api\wwwroot\img\SetPlaylistShowcase1.png)
<br>
<br>
<br>
<br>
<br>
<br>
![Screenshot 1](SetPlayList.Api\wwwroot\img\SetPlaylistShowcase2.png)

## How to Run Locally
To run the project on your computer, follow these steps:

1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/srebrek/SetPlayList
    cd SetPlayList
    ```
2.  **Configure API Keys:**
    *   Obtain your API keys for the Spotify API and Setlist.fm API.
    *   Add them to your `appsettings.json` file or as environment variables, according to your project's configuration.
    *   By default RedirectUri should be "https://localhost:7101/auth/spotify/callback".
3.  **Build and Run the Project:**
    ```bash
    dotnet build
    dotnet run
    ```
    Alternatively, you can run the project from an IDE such as Visual Studio.
4.  **Open in Browser:**
    The application will be available at `https://localhost:7101`.
