# Custom Networking Solution in C#
## Description
A small FPS game project to practice custom networking solutions in C#. Players can move and jump around while shooting other connected players or AI bots.

I followed the tutorials of [Tom Weiland](https://www.youtube.com/c/TomWeiland/featured) who helped me gain confidence in networking programming as it is my first time. I plan to use my newly acquired networking skills to implement a chat application and a basic multi-player game of my own.

## How to Play
> WASD to move.\
> Space to jump.\
> Rotate by moving moves as in any FPS game.\
> Left mouse click to shoot.\
> Press "G" key to throw projectile if you have collected any.

## What I Practised Implementing the Project
* TCP/UDP clients.
* Server-side Logic (The game logic is updated in the server while the output is sent to the clients to render!).
* Server-side Physics.
* Server-side AI.
* Communicating/syncronizing game events such as position change, shooting, items between players.

### Possible Improvements
* Matchmaking.
    *  Players should be able to host games over local-wifi or find public games.
* Larger map.
* Spawn Screen / Wait Room.
* In-game chat.
* Game settings
    *  Players should be able to customize game-room data when hosting their own games.

### Current Bugs
* Client throws an error when they destroy a bot with a projectile.

