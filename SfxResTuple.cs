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
     * Class to associate the List of PlayerTuple's
     * playing the associated buffer.
     */
    public class SfxResTuple : IDisposable
    {
        /*
         * Sound buffer.
         */
        public byte[] Buffer { get; set; }
        /*
         * List of PlayerTuple's currently playing the
         * audio resource at the Buffer.
         */
        public List<PlayerTuple> PlayerTupleList { get; set; }

        /*
         * Default constructor that takes the buffer
         * to initialize it.
         * 
         * \param buffer Byte array that contains an
         * audio resource.
         */
        public SfxResTuple(byte[] buffer)
        {
            Buffer = buffer;
            PlayerTupleList = new List<PlayerTuple>();
        }

        /*
         * Plays the sound effect at the Buffer
         * 
         * \param looping true to play it looping until
         * manually stopped or false to play it just once.
         */
        public void PlaySfx(bool looping)
        {
            var playerTuple = new PlayerTuple(Buffer, looping);
            if (!looping)
            {
                playerTuple.Player.PlaybackStopped += (sender, evArgs) =>
                {
                    PlayerTupleList.Remove(playerTuple);
                    playerTuple.Dispose();
                };
            }
            PlayerTupleList.Add(playerTuple);
            playerTuple.Play();
        }

        /*
         * Stops all the playback of this SFX.
         */
        public void StopSfx()
        {
            foreach (var playerTuple in PlayerTupleList)
            {
                playerTuple.Dispose();
            }
            PlayerTupleList.Clear();
        }

        /*
         * Stops only the playback of this SFX that
         * conforms to the specified looping state.
         * 
         * \param looping true if stopping only the
         * looping streams, false if stopping only
         * the singe time playing streams.
         */
        public void StopSfx(bool looping)
        {
            var stoppingPlayers = PlayerTupleList.Where(x => x.IsLooping == looping);
            foreach (var stoppingPlayer in stoppingPlayers)
            {
                stoppingPlayer.Stop();
                stoppingPlayer.Dispose();
                PlayerTupleList.Remove(stoppingPlayer);
            }
        }

        /*
         * Calls StopSfx() to dispose of all this SFX's players.
         */
        private void ReleaseUnmanagedResources()
        {
            StopSfx();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~SfxResTuple()
        {
            ReleaseUnmanagedResources();
        }
    }
}
