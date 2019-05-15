using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        //Scope of this goes in Programmable Block
        VRageMath.Color colorRed = new VRageMath.Color(255, 0, 0, 255);
        VRageMath.Color colorWhite = new VRageMath.Color(255, 255, 255, 255);
        public void Main(string argument, UpdateType updateSource)
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            IMyTextPanel display = GridTerminalSystem.GetBlockWithName("LCD Panel [Status]") as IMyTextPanel;

            IMyBlockGroup hangarLights = GridTerminalSystem.GetBlockGroupWithName("Hangar Lights");

            IMyBlockGroup hangarDoors = GridTerminalSystem.GetBlockGroupWithName("Hangar Doors");

            List<IMyTerminalBlock> doors = new List<IMyTerminalBlock>();
            hangarDoors.GetBlocksOfType<IMyAirtightHangarDoor>(doors);

            bool missing = false;

            if (hangarDoors == null) //Returns if doors not found
            {
                Echo("Doors not found");
                missing = true;
            }

            List<IMyTerminalBlock> lights = new List<IMyTerminalBlock>();
            hangarLights.GetBlocksOfType<IMyInteriorLight>(lights);

            if (hangarLights == null) //Returns if lights not found
            {
                Echo("Lights not found");
                missing = true;
            }

            IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName("[Hangar] Timer Block") as IMyTimerBlock;

            if (timer == null) //Returns if timer not found
            {
                Echo("Timer not found");
                missing = true;
            }

            IMyAirVent vent = GridTerminalSystem.GetBlockWithName("[Hangar] Air Vent") as IMyAirVent;

            if (vent == null) //Returns if vent not found
            {
                Echo("Vent not found");
                missing = true;
            }

            IMySoundBlock speaker = GridTerminalSystem.GetBlockWithName("[Hangar] Sound Block") as IMySoundBlock;

            if (speaker == null) //Returns if speaker not found
            {
                Echo("Speaker not found");
                missing = true;
            }

            if (missing == true)
            {
                return;
            }

            UnifyLights(lights); //Turns all lights on

            speaker.SelectedSound = "Alert 1"; //Set speaker sound

            if (!timer.IsCountingDown) //Program will skip over if the timer is counting down
            {
                if (timer.TriggerDelay == 10) //Program will disable timer if the timer is set to 10 (end of hanagar door stage).
                {
                    NormalLights(lights);
                    display.FontSize = 4.4F;
                    Echo("All is well");
                    display.WritePublicTitle("All is well");
                    timer.Enabled = true;
                    timer.TriggerDelay = 15;
                    speaker.LoopPeriod = 10;
                    speaker.Stop();
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                }
                else //Program will start hangar toggle process else wise
                {
                    if (vent.CanPressurize && !vent.Depressurize) //If the vent needs to depressurize, start depressurize sequence
                    {
                        vent.Depressurize = true;
                        timer.Enabled = true;
                        timer.TriggerDelay = 15;
                        timer.StartCountdown();
                        speaker.LoopPeriod = 25;
                        speaker.Play();
                        WarningLights(lights);
                        return;
                    }
                    WarningLights(lights);
                    ToggleDoors(doors);
                    display.FontSize = 3;
                    Echo("!!!CAUTION!!!");
                    display.WritePublicTitle("!!!CAUTION!!!");
                    timer.TriggerDelay = 10;
                    timer.StartCountdown();
                    vent.Depressurize = false;
                    if (speaker.LoopPeriod != 25)
                    {
                        speaker.LoopPeriod = 10;
                        speaker.Play();
                    }
                }
            }
        }

        public void ToggleDoors(List<IMyTerminalBlock> doors)
        {
            int openCount = 0;

            foreach (IMyAirtightHangarDoor door in doors) //Counts how many doors are open
            {
                if (door.Status == DoorStatus.Open)
                {
                    openCount++;
                }
            }

            if (openCount >= doors.Count / 2) //If most doors are open, then close the doors
            {
                foreach (IMyAirtightHangarDoor door in doors)
                {
                    door.CloseDoor();
                }
            }
            else
            {
                foreach (IMyAirtightHangarDoor door in doors) //If most are closed, then open the doors
                {
                    door.OpenDoor();
                }
            }
        }

        public void NormalLights(List<IMyTerminalBlock> lights) //"Default" lights
        {
            foreach (IMyInteriorLight light in lights)
            {
                light.Intensity = 10;
                light.Radius = 10;
                light.Color = colorWhite;
                light.BlinkIntervalSeconds = 0;
                light.BlinkLength = 0;
            }
        }

        public void WarningLights(List<IMyTerminalBlock> lights) //Flashing red warning lights
        {
            foreach (IMyInteriorLight light in lights)
            {
                light.Intensity = 1;
                light.Radius = 7;
                light.Color = colorRed;
                light.BlinkIntervalSeconds = 4;
                light.BlinkLength = 75;
            }
        }

        public void UnifyLights(List<IMyTerminalBlock> lights) //Sets all hangar lights to on
        {
            foreach (IMyInteriorLight light in lights)
            {
                light.Enabled = true;
            }
        }
    }
}
