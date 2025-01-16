using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class HttpService
{
    private static readonly string userService = "http://localhost:3010";
    private static readonly string getService = "http://localhost:3001";
    private static readonly string sensorService = "http://localhost:3007";
    private static readonly string spendService = "http://localhost:3008";
    private static readonly string videogame = "WealthQuest";

    private static Dictionary<string, string> urlDictionary = new Dictionary<string, string>
    {
        { "login", $"{userService}/player/" },
        { "getPlayerById", $"{userService}/players/" },
        { "getPlayerAttributes", $"{getService}/player_all_attributes/" },
        { "videogames", $"{sensorService}/videogames" },
        { "mechanicsOfVideogame", $"{sensorService}/mechanics_of_videogame/" },
        { "spendAttributes", $"{spendService}/spend_attributes_apis" }
    };

    # region Login & Authenticator

    // Método para manejar el login
    public static async Task<bool> Login(string name, string password)
    {
        try
        {
            // Primera solicitud: Obtener el ID del jugador
            int playerId = await GetPlayerId(name, password);

            if (playerId < 0)
            {
                Debug.LogError("ID del jugador no encontrado o inválido.");
                return false;
            }

            // Segunda solicitud: Obtener los datos del jugador
            BGamesProfile profile = await GetPlayerData(playerId);

            if (profile != null)
            {
                // Tercera solicitud: Obtener atributos del jugador y actualizar points
                int points = await GetPlayerPoints(playerId);
                profile.points = points;

                ProfileUser.LoginBGames(profile);
                Debug.Log($"Jugador encontrado: ID: {profile.id}, Name: {profile.name}, Points: {profile.points}");
                return true;
            }
            else
            {
                Debug.LogError("No se pudo obtener los datos del jugador.");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error en Login: {ex.Message}");
            return false;
        }
    }

    // Metodo para autenticar al jugador
    public static async Task<bool> Authenticator(int id)
    {
        try
        {
            // Obtener los datos básicos del jugador
            BGamesProfile profile = await GetPlayerData(id);

            if (profile != null)
            {
                // Obtener los puntos basados en el atributo "Cognitivo"
                int points = await GetPlayerPoints(id);
                profile.points = points;

                // Guardar el perfil actualizado
                ProfileUser.LoadBGamesPlayer(profile);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error en Authenticator: {ex.Message}");
            return false;
        }
    }

    // Método privado para obtener el ID del jugador
    private static async Task<int> GetPlayerId(string name, string password)
    {
        string loginUrl = $"{urlDictionary["login"]}{name}/{password}";
        using (UnityWebRequest request = UnityWebRequest.Get(loginUrl))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error en GetPlayerId: {request.error}");
                return 0;
            }
            else
            {
                int playerId = JsonConvert.DeserializeObject<int>(request.downloadHandler.text);
                Debug.Log($"ID del jugador obtenido: {playerId}");
                return playerId;
            }
        }
    }

    // Método privado para obtener los datos del jugador
    private static async Task<BGamesProfile> GetPlayerData(int playerId)
    {
        string getPlayerUrl = $"{urlDictionary["getPlayerById"]}{playerId}";
        using (UnityWebRequest request = UnityWebRequest.Get(getPlayerUrl))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                return null;
            }
            else
            {
                try
                {
                    // Deserializar en una lista de BGamesPlayer
                    List<BGamesPlayer> players = JsonConvert.DeserializeObject<List<BGamesPlayer>>(request.downloadHandler.text);

                    if (players != null && players.Count > 0)
                    {
                        BGamesPlayer player = players[0];

                        // Mapear los datos al perfil de BGames
                        BGamesProfile profile = new BGamesProfile
                        {
                            id = player.id_players,
                            name = player.name,
                            points = 0
                        };

                        return profile;
                    }
                    else
                    {
                        Debug.LogError("La lista de jugadores está vacía o es nula.");
                        return null;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error al deserializar los datos del jugador: {ex.Message}");
                    return null;
                }
            }
        }
    }

    // Método para obtener los puntos del jugador
    private static async Task<int> GetPlayerPoints(int playerId)
    {
        string url = $"{urlDictionary["getPlayerAttributes"]}{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield(); // Esperar la operación
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error en GetPlayerPoints: {request.error}");
                return 0;
            }
            else
            {
                try
                {
                    // Deserializar la respuesta en una lista de atributos
                    List<BGamesAttributes> attributes = JsonConvert.DeserializeObject<List<BGamesAttributes>>(request.downloadHandler.text);

                    // Buscar el atributo "Cognitivo" y devolver su valor
                    BGamesAttributes cognitiveAttribute = attributes.Find(attr => attr.name == "Cognitivo");
                    if (cognitiveAttribute != null)
                    {
                        return cognitiveAttribute.data;
                    }
                    else
                    {
                        Debug.LogWarning("Atributo Cognitivo no encontrado.");
                        return 0;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error al deserializar los atributos del jugador: {ex.Message}");
                    return 0;
                }
            }
        }
    }

    #endregion

    # region Spend Attributes

    // Método principal para gestionar la funcionalidad
    public static async Task<bool> SpendPoints(int points)
    {
        try
        {
            // Obtener el ID del perfil del jugador
            int idPlayer = ProfileUser.bGamesProfile.id;

            // Paso 1: Obtener el ID del videojuego
            int idVideogame = await GetVideogameId(videogame);
            if (idVideogame < 0)
            {
                Debug.LogError($"No se encontró el videojuego {videogame}.");
                return false;
            }

            // Paso 2: Obtener el ID de la mecánica modificable
            int idModifiableMechanic = await GetModifiableMechanicId(idVideogame);
            if (idModifiableMechanic < 0)
            {
                Debug.LogError($"No se encontró una mecánica modificable para {videogame}.");
                return false;
            }

            // Paso 3: Realizar el POST para consumir los puntos
            bool success = await PostSpendAttributes(idPlayer, idVideogame, idModifiableMechanic, points);

            if (success)
            {
                Debug.Log("Puntos consumidos exitosamente.");
                ProfileUser.bGamesProfile.points--;
                return true;
            }
            else
            {
                Debug.LogError("No se pudieron consumir los puntos.");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error en SpendPoints: {ex.Message}");
            return false;
        }
    }

    // Método para obtener el ID del videojuego
    private static async Task<int> GetVideogameId(string videogameName)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(urlDictionary["videogames"]))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error en GetVideogameId: {request.error}");
                return 0;
            }
            else
            {
                try
                {
                    List<Videogame> videogames = JsonConvert.DeserializeObject<List<Videogame>>(request.downloadHandler.text);
                    Videogame targetGame = videogames.Find(game => game.name == videogameName);

                    if (targetGame != null)
                    {
                        Debug.Log($"ID del videojuego encontrado: {targetGame.id_videogame}");
                        return targetGame.id_videogame;
                    }

                    Debug.LogWarning("El videojuego no fue encontrado en la lista.");
                    return 0;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error al deserializar la lista de videojuegos: {ex.Message}");
                    return 0;
                }
            }
        }
    }

    // Método para obtener el ID de la mecánica modificable
    private static async Task<int> GetModifiableMechanicId(int idVideogame)
    {
        string url = $"{urlDictionary["mechanicsOfVideogame"]}{idVideogame}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error en GetModifiableMechanicId: {request.error}");
                return 0;
            }
            else
            {
                try
                {
                    List<ModifiableMechanic> mechanics = JsonConvert.DeserializeObject<List<ModifiableMechanic>>(request.downloadHandler.text);

                    if (mechanics != null && mechanics.Count > 0)
                    {
                        Debug.Log($"ID de la mecánica modificable encontrada: {mechanics[0].id_modifiable_mechanic}");
                        return mechanics[0].id_modifiable_mechanic;
                    }

                    Debug.LogWarning("No se encontraron mecánicas modificables.");
                    return 0;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error al deserializar las mecánicas modificables: {ex.Message}");
                    return 0;
                }
            }
        }
    }

    // Método para realizar el POST a spend_attributes_apis
    private static async Task<bool> PostSpendAttributes(int idPlayer, int idVideogame, int idModifiableMechanic, int data)
    {
        var payload = new
        {
            id_player = idPlayer,
            id_videogame = idVideogame,
            id_modifiable_mechanic = idModifiableMechanic,
            data
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(urlDictionary["spendAttributes"], "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error en PostSpendAttributes: {request.error}");
                return false;
            }
            else
            {
                Debug.Log("POST exitoso a spend_attributes_apis.");
                return true;
            }
        }
    }

    #endregion

}
