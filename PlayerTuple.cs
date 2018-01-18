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
using System.IO;
using NAudio.Wave;

namespace SoundEngine
{
    /*
     * Wrapper class for player functionality that
     * unifies the player object, the file reader
     * and the memory stream containing the file.
     */
    class PlayerTuple : IDisposable
    {
        /*
         * Player object implemented through NAudio's
         * WaveOutEvent.
         */
        public WaveOutEvent Player { get; set; }
        /*
         * Mp3Reader implemented through NAudio's
         * Mp3FileReader
         */
        public Mp3FileReader Mp3Reader { get; set; }
        /*
         * MemoryStream containing the audio resource.
         */
        public MemoryStream AudioStream { get; set; }
        /*
         * Controls the behaviour of the default
         * Playback Stopped event handler,
         * if set to true, the sound will loop upon
         * the raising of this event by the Player
         * object.
         */
        public bool IsLooping { get; set; }

        /*
         * Constructs the PlayerTuple through creating a MemoryStream
         * from the provided buffer.
         * 
         * \param baseBuffer The byte array containing the audio
         * file.
         * \param isLooping Sets the behavior of the default
         * PlaybackStopped callback as explained in the
         * IsLooping property's docstring.
         */
        public PlayerTuple(byte[] baseBuffer, bool isLooping = false)
        {
            Player = new WaveOutEvent();
            AudioStream = new MemoryStream(baseBuffer);
            Mp3Reader = new Mp3FileReader(AudioStream);
            IsLooping = isLooping;

            Player.Init(Mp3Reader);

            Player.PlaybackStopped += LoopingCallback;
        }

        /*
         * Default event handler of the Player's PlaybackStopped event.
         */
        protected void LoopingCallback(object sender, EventArgs evArgs)
        {
            if (IsLooping)
            {
                Mp3Reader.Position = 0;
                Player.Play();
            }
        }

        /*
         * Wrapper for the Players Play method.
         */
        public void Play()
        {
            Player.Play();
        }

        /*
         * Wrapper for the Players Pause method.
         */
        public void Pause()
        {
            Player.Pause();
        }

        /*
         * Wrapper for the Players Stop method
         * that overrides the looping behaviour
         * to guarantee that it will stop.
         */
        public void Stop()
        {
            IsLooping = false;
            Player.Stop();
        }

        /*
         * \brief Sets the reader's position to the specified time.
         * 
         * Throws an exception if the time is greater than the resource's
         * TotalTime.
         * 
         * \param time The time to set the cursor to.
         */
        public void GoTo(TimeSpan time)
        {
            if (time > Mp3Reader.TotalTime)
            {
                throw new Exception("Time is greater than the source's size!");
            }
            Mp3Reader.CurrentTime = time;
        }

        /*
         * Disposes of the Player, Reader and Stream objects.
         */
        public void Dispose()
        {
            Stop();
            Player.Dispose();
            Mp3Reader.Dispose();
            AudioStream.Dispose();
        }
    }
}
