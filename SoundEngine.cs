/*
 * MIT License
 * 
 * Copyright (c) 2018 Pedro Linhares
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace NSoundEngine
{
    /*
     * Singleton class for the general SoundEngine logic.
     */
    public class SoundEngine
    {
        /*
         * Internal Instance variable
         */
        protected static SoundEngine SingletonInstance { get; set; }
        /*
         * Gets the singleton's instance if it was already created
         * or creates it otherwise.
         */
        public static SoundEngine Instance => SingletonInstance ?? (SingletonInstance = new SoundEngine());

        /* 
         * Instantiate and populate this dictionary previously to calling
         * the Instance for the first time for its contents to be loaded
         * as audio resources at construction time.
         * Its key entries must have one of the valid prefixes.
         * Or an exception will be thrown at construction time.
         */
        public static Dictionary<string, byte[]> InitialAudioResources { get; set; }

        /*
         * Dictionary of the bgm player structures.
         */
        protected Dictionary<string, PlayerTuple> BgmPlayers { get; set; }
        /*
         * Dictionary of the SFX Player list of structures
         * per audio resource.
         */
        protected Dictionary<string, SfxResTuple> SfxResPlayers { get; set; }

        /*
         * Mandatory prefix for BGM audio resources.
         */
        public const string BGM_PREFIX = "bgm.";
        /*
         * Mandatory prefix for sfx audio resources.
         */
        public const string SFX_PREFIX = "sfx.";

        /*
         * Default no prefix exception message string.
         */
        protected const string EXCEPTION_NO_PREFIX = "The key must contain the default bgm or sfx prefix!";
        /*
         * Default no BGM prefix exception message string.
         */
        protected const string EXCEPTION_NO_BGM_PREFIX = "The key must contain the default bgm prefix!";
        /*
         * Default no SFX prefix exception message string.
         */
        protected const string EXCEPTION_NO_SFX_PREFIX = "The key must contain the default sfx prefix!";

        /*
         * Default key not found exception message string.
         */
        protected const string EXCEPTION_KEY_NOT_FOUND = "The key was not found in the audio resources!";

        /*
         * Singleton constructor that initializes the variables and
         * does initial population of audio resources through
         * the InitialAudioResources dictionary.
         */
        protected SoundEngine()
        {
            BgmPlayers = new Dictionary<string, PlayerTuple>();
            SfxResPlayers = new Dictionary<string, SfxResTuple>();

            if (InitialAudioResources == null) return;
            foreach (var item in InitialAudioResources)
            {
                AddAudioResource(item.Key, item.Value);
            }
        }

        /*
         * \brief Method to add new audio resources to the Sound Engine.
         * 
         * Will throw an exception if the key doesn't contain one of the
         * valid prefixes.
         * 
         * \param key The audio resource's key, that must contain either
         * BGM_PREFIX or SFX_PREFIX
         * \param buffer The byte array that represents the buffer that
         * holds the MP3 file, initially meant to be used with
         * Embedded Resources.
         */
        public void AddAudioResource(string key, byte[] buffer)
        {
            if (key.Contains(BGM_PREFIX))
            {
                BgmPlayers.Add(key, new PlayerTuple(buffer, false));
            }
            else if (key.Contains(SFX_PREFIX))
            {
                SfxResPlayers.Add(key, new SfxResTuple(buffer));
            }
            else
            {
                throw new Exception(EXCEPTION_NO_PREFIX);
            }
        }        

        /*
         * \brief Removes the audio resource identified by the
         * provided key, freeing memory.
         * 
         * Willl throw an exception if either the key doesn't contain
         * a valid prefix or if it is not found.
         * 
         * \param key The resource's key that must contain either
         * BGM_PREFIX or SFX_PREFIX.
         */
        public void RemoveAudioResource(string key)
        {
            if (key.Contains(BGM_PREFIX))
            {
                if (!BgmPlayers.Keys.Contains(key))
                {
                    throw new Exception(EXCEPTION_KEY_NOT_FOUND);
                }
                BgmPlayers[key].Stop();
                BgmPlayers[key].Dispose();
                BgmPlayers.Remove(key);
            }
            else if (key.Contains(SFX_PREFIX))
            {
                if (!SfxResPlayers.Keys.Contains(key))
                {
                    throw new Exception(EXCEPTION_KEY_NOT_FOUND);
                }
                SfxResPlayers[key].Dispose();
                SfxResPlayers.Remove(key);
            }
            else
            {
                throw new Exception(EXCEPTION_NO_PREFIX);
            }
        }

        /*
         * \brief Plays a sound, BGM or SFX, identified by its key
         * 
         * Will throw an exception if either the key is non conformant
         * to the default prefixes or if it is not found.
         * 
         * \param key The resource's key that must contain either
         * BGM_PREFIX or SFX_PREFIX.
         * \param looping Identifies if the sound must be played
         * looping (until manually stopped) (true) or not (false).
         */
        public void PlaySound(string key, bool looping)
        {
            if (key.Contains(BGM_PREFIX))
            {
                if (!BgmPlayers.Keys.Contains(key))
                {
                    throw new Exception(EXCEPTION_KEY_NOT_FOUND);
                }
                var playerTuple = BgmPlayers[key];
                playerTuple.IsLooping = looping;
                playerTuple.Play();
            }
            else if (key.Contains(SFX_PREFIX))
            {
                if (!SfxResPlayers.Keys.Contains(key))
                {
                    throw new Exception(EXCEPTION_KEY_NOT_FOUND);
                }
                SfxResPlayers[key].PlaySfx(looping);
            }
            else
            {
                throw new Exception(EXCEPTION_NO_PREFIX);
            }
        }

        /*
         * Stops all the BGM.
         */
        public void StopBgm()
        {
            foreach (var playerTuple in BgmPlayers.Values)
            {
                playerTuple.Stop();
            }
        }

        /*
         * \brief Stops playback of the specified BGM.
         * 
         * Will throw an exception if either the key is non
         * conformant to the key prefixes or if it is not
         * found.
         * 
         * \param key The resource's key that must contain either
         * BGM_PREFIX or SFX_PREFIX.
         */
        public void StopBgm(string key)
        {
            if (!key.Contains(BGM_PREFIX))
            {
                throw new Exception(EXCEPTION_NO_BGM_PREFIX);
            }
            if (!BgmPlayers.Keys.Contains(key))
            {
                throw new Exception(EXCEPTION_KEY_NOT_FOUND);
            }

            BgmPlayers[key].Stop();
        }

        /*
         * Stops all SFX playback.
         */
        public void StopSfx()
        {
            foreach (var sfxResTuple in SfxResPlayers.Values)
            {
                sfxResTuple.StopSfx();
            }
        }

        /*
         * \brief Stops playback of the SFX specified by it's key.
         * 
         * Throws an exception if the key doesn't contain one of
         * the necessary prefixes or if it is not found.
         * 
         * \param key The resource's key that must contain either
         * BGM_PREFIX or SFX_PREFIX.
         * \param onlyLooping
         * \param onlyNotLooping
         */
        public void StopSfx(string key, bool onlyLooping = false, bool onlyNotLooping = false)
        {
            if (!key.Contains(SFX_PREFIX))
            {
                throw new Exception(EXCEPTION_NO_SFX_PREFIX);
            }
            if (!SfxResPlayers.Keys.Contains(key))
            {
                throw new Exception(EXCEPTION_KEY_NOT_FOUND);
            }

            if (onlyLooping)
            {
                SfxResPlayers[key].StopSfx(true);
            }
            else if (onlyNotLooping)
            {
                SfxResPlayers[key].StopSfx(false);
            }
            else
            {
                SfxResPlayers[key].StopSfx();
            }
        }

        /*
         * Returns all currently valid BGM Keys.
         */
        public string[] GetBgmKeys()
        {
            return BgmPlayers.Keys.ToArray();
        }

        /*
         * Returns all currently valid SFX Keys.
         */
        public string[] GetSfxKeys()
        {
            return SfxResPlayers.Keys.ToArray();
        }

        /*
         * \brief Gets the underlying PlayerTuple list of the specified
         * audio resource.
         * 
         * If it is a BGM than it is a single entry list, if it is
         * a SFX it may contain any amount of entries.
         * 
         * Throws an exception when the key is not found or doesn't
         * contain one of the mandatory prefixes.
         * 
         * \param key The resource's key that must contain either
         * BGM_PREFIX or SFX_PREFIX.
         */
        public List<PlayerTuple> GetPlayerTuple(string key)
        {
            if (key.Contains(BGM_PREFIX))
            {
                if (!BgmPlayers.Keys.Contains(key))
                {
                    throw new Exception(EXCEPTION_KEY_NOT_FOUND);
                }
                return new List<PlayerTuple>() { BgmPlayers[key] };
            }

            if (!key.Contains(SFX_PREFIX))
                throw new Exception(EXCEPTION_NO_PREFIX);

            if (!SfxResPlayers.Keys.Contains(key))
            {
                throw new Exception(EXCEPTION_KEY_NOT_FOUND);
            }

            return SfxResPlayers[key].PlayerTupleList;
        }
    }
}
