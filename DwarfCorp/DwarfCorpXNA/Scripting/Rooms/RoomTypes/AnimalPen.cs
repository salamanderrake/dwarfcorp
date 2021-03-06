// BalloonPort.cs
// 
//  Modified MIT License (MIT)
//  
//  Copyright (c) 2015 Completely Fair Games Ltd.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// The following content pieces are considered PROPRIETARY and may not be used
// in any derivative works, commercial or non commercial, without explicit 
// written permission from Completely Fair Games:
// 
// * Images (sprites, textures, etc.)
// * 3D Models
// * Sound Effects
// * Music
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DwarfCorp
{
    [JsonObject(IsReference = true)]
    public class AnimalPen : Room
    {
        [JsonIgnore]
        public static string AnimalPenName { get { return "Animal Pen"; } }
        [JsonIgnore]
        public static RoomData AnimalPenData { get { return RoomLibrary.GetData(AnimalPenName); } }

        public static RoomData InitializeData()
        {
            Dictionary<Resource.ResourceTags, Quantitiy<Resource.ResourceTags>> resources =
                new Dictionary<Resource.ResourceTags, Quantitiy<Resource.ResourceTags>>();
            resources[Resource.ResourceTags.Soil] = new Quantitiy<Resource.ResourceTags>()
            {
                ResourceType = Resource.ResourceTags.Soil,
                NumResources = 4
            };


            return new RoomData(AnimalPenName, 12, "Dirt", resources, new List<RoomTemplate>(), new Gui.TileReference("rooms", 13))
            {
                Description = "Animals can be wrangled and stored here."
            };
        }

        public string Species = "";

        public AnimalPen()
        {

        }
        
        public AnimalPen(bool designation, IEnumerable<VoxelHandle> designations, WorldManager world, Faction faction) :
            base(designation, designations, AnimalPenData, world, faction)
        {
        }

        public AnimalPen(IEnumerable<VoxelHandle> voxels, WorldManager world, Faction faction) :
            base(voxels, AnimalPenData, world, faction)
        {
            OnBuilt();
        }
        public override void OnBuilt()
        {
            foreach (
                var fence in
                    Fence.CreateFences(World.ComponentManager, ContentPaths.Entities.DwarfObjects.fence, Designations,
                        false))
            {
                AddBody(fence);
                fence.Manager.RootComponent.AddChild(fence);
            }
        }

        public IEnumerable<Act.Status> AddAnimal(Body animal, Faction faction)
        {
            AddBody(animal);
            BoundingBox animalBounds = GetBoundingBox();
            animalBounds = animalBounds.Expand(-0.25f);
            animalBounds.Max.Y += 2;
            animalBounds.Min.Y -= 0.25f;
            animal.GetComponent<Physics>().IsReserved = false;
            animal.GetComponent<CreatureAI>().PositionConstraint = animalBounds;
            faction.WrangleDesignations.Remove(animal.GetComponent<Physics>());
           yield return Act.Status.Success;
        }

        public IEnumerable<Act.Status> RemoveAnimal(Body animal)
        {
            if (!ZoneBodies.Contains(animal))
            {
                yield return Act.Status.Fail;
                yield break;
            }
            ZoneBodies.Remove(animal);
            animal.GetComponent<CreatureAI>().ResetPositionConstraint();
            Species = animal.GetComponent<Creature>().Species;
            yield return Act.Status.Success;
        }

        public override void Update()
        {
            if (ZoneBodies.Count > 0)
            {
                ZoneBodies.RemoveAll(body => body.IsDead);
                if (ZoneBodies.Count == 0)
                {
                    Species = "";
                }
            }
            base.Update();
        }
    }
}
