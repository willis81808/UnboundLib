﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnboundLib.GameModes
{
    /// <inheritdoc/>
    public abstract class GameModeHandler<T> : IGameModeHandler<T> where T : MonoBehaviour
    {
        public T GameMode {
            get
            {
                return GameModeManager.GetGameMode<T>(this.gameModeId);
            }
        }

        MonoBehaviour IGameModeHandler.GameMode
        {
            get
            {
                return this.GameMode;
            }
        }

        public abstract GameSettings Settings { get; protected set; }
        public abstract string Name { get; }

        // Used to find the correct game mode from scene
        private readonly string gameModeId;

        private Dictionary<string, List<Func<IGameModeHandler, IEnumerator>>> hooks = new Dictionary<string, List<Func<IGameModeHandler, IEnumerator>>>();

        protected GameModeHandler(string gameModeId)
        {
            this.gameModeId = gameModeId;
        }

        public void AddHook(string key, Func<IGameModeHandler, IEnumerator> action)
        {
            if (action == null)
            {
                return;
            }

            // Case-insensitive keys for QoL
            key = key.ToLower();

            if (!this.hooks.ContainsKey(key))
            {
                this.hooks.Add(key, new List<Func<IGameModeHandler, IEnumerator>> { action });
            }
            else
            {
                this.hooks[key].Add(action);
            }
        }

        public void RemoveHook(string key, Func<IGameModeHandler, IEnumerator> action)
        {
            this.hooks[key.ToLower()].Remove(action);
        }

        public IEnumerator TriggerHook(string key)
        {
            List<Func<IGameModeHandler, IEnumerator>> hooks;
            this.hooks.TryGetValue(key.ToLower(), out hooks);

            if (hooks != null)
            {
                foreach (var hook in hooks)
                {
                    yield return hook(this);
                }
            }
        }

        public void SetSettings(GameSettings settings)
        {
            this.Settings = settings;

            foreach (var entry in this.Settings)
            {
                this.ChangeSetting(entry.Key, entry.Value);
            }
        }

        public virtual void ChangeSetting(string name, object value)
        {
            var newSettings = new GameSettings();

            foreach (var entry in this.Settings)
            {
                newSettings.Add(entry.Key, entry.Key == name ? value : entry.Value);
            }

            this.Settings = newSettings;
        }

        public abstract void PlayerJoined(Player player);

        public abstract void PlayerDied(Player killedPlayer, int playersAlive);

        public abstract TeamScore GetTeamScore(int teamID);

        public abstract void SetTeamScore(int teamID, TeamScore score);

        public abstract void SetActive(bool active);

        public abstract void StartGame();

        public abstract void ResetGame();
    }
}
