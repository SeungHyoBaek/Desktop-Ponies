﻿namespace DesktopSprites.SpriteManagement
{
    /// <summary>
    /// Defines a class the controls a collection of sprites and displays them using a
    /// <see cref="T:DesktopSprites.SpriteManagement.ISpriteCollectionView"/>.
    /// </summary>
    public interface ISpriteCollectionController
    {
        /// <summary>
        /// Gets a value indicating whether the active updating of sprites has begun.
        /// </summary>
        bool Started { get; }
        /// <summary>
        /// Starts actively updating the sprites in the collection.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops updating sprites in the collection.
        /// </summary>
        void Finish();
    }
}
