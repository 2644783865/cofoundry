﻿using Cofoundry.Core.EntityFramework;
using Cofoundry.Core.Validation;
using Cofoundry.Domain;
using Cofoundry.Domain.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofoundry.Plugins.ImageFiles.SystemDotDrawing
{
    public class SystemDrawingImageAssetFileService : IImageAssetFileService
    {
        private const string ASSET_FILE_CONTAINER_NAME = "Images";

        private static readonly Dictionary<ImageFormat, string[]> _permittedImageFileExtensionMap = new Dictionary<ImageFormat, string[]>() {
            { ImageFormat.Jpeg, new string[] { ".jpg", ".jpeg" } },
            { ImageFormat.Png, new string[] { ".png" } },
            { ImageFormat.Gif, new string[] { ".gif" } }
        };

        #region constructor

        private readonly CofoundryDbContext _dbContext;
        private readonly IFileStoreService _fileStoreService;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public SystemDrawingImageAssetFileService(
            CofoundryDbContext dbContext,
            IFileStoreService fileStoreService,
            ITransactionScopeFactory transactionScopeFactory
            )
        {
            _dbContext = dbContext;
            _fileStoreService = fileStoreService;
            _transactionScopeFactory = transactionScopeFactory;
        }

        #endregion

        public async Task SaveAsync(IUploadedFile uploadedFile, ImageAsset imageAsset, string propertyName)
        {
            Image imageFile = null;

            using (var inputSteam = await uploadedFile.OpenReadStreamAsync())
            {
                try
                {
                    imageFile = Image.FromStream(inputSteam);
                }
                catch (ArgumentException ex)
                {
                    // We'll get an argument exception if the image file is invalid
                    // so lets check to see if we can identify if it is an invalid file type and show that error
                    // This might not always be the case since a file extension or mime type might not be supplied.
                    var ext = Path.GetExtension(uploadedFile.FileName);
                    if ((!string.IsNullOrEmpty(ext) && !ImageAssetConstants.PermittedImageTypes.ContainsKey(ext))
                        || (!string.IsNullOrEmpty(uploadedFile.MimeType) && !ImageAssetConstants.PermittedImageTypes.ContainsValue(uploadedFile.MimeType)))
                    {
                        throw new PropertyValidationException("The file is not a supported image type.", propertyName);
                    }

                    throw;
                }

                using (imageFile) // validate image file
                {
                    bool requiredReEncoding = !_permittedImageFileExtensionMap.ContainsKey(imageFile.RawFormat);
                    bool isNew = imageAsset.ImageAssetId < 1;

                    var imageFormat = requiredReEncoding ? ImageFormat.Jpeg : imageFile.RawFormat;

                    imageAsset.Width = imageFile.Width;
                    imageAsset.Height = imageFile.Height;
                    imageAsset.Extension = _permittedImageFileExtensionMap[imageFormat].First().TrimStart('.');
                    imageAsset.FileSize = Convert.ToInt32(inputSteam.Length);

                    // Save at this point if it's a new image
                    if (isNew)
                    {
                        await _dbContext.SaveChangesAsync();
                    }

                    using (var scope = _transactionScopeFactory.Create(_dbContext))
                    {
                        var fileName = Path.ChangeExtension(imageAsset.ImageAssetId.ToString(), imageAsset.Extension);

                        if (requiredReEncoding)
                        {
                            // Convert the image to jpg
                            using (var outputStream = new MemoryStream())
                            {
                                imageFile.Save(outputStream, imageFormat);
                                CreateFile(isNew, fileName, outputStream);
                                // recalculate size and save
                                imageAsset.FileSize = Convert.ToInt32(outputStream.Length);
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            // Save the raw file directly
                            CreateFile(isNew, fileName, inputSteam);
                        }

                        scope.Complete();
                    };
                }
            }
        }

        private void CreateFile(bool isNew, string fileName, Stream outputStream)
        {
            if (isNew)
            {
                _fileStoreService.Create(ASSET_FILE_CONTAINER_NAME, fileName, outputStream);
            }
            else
            {
                _fileStoreService.CreateOrReplace(ASSET_FILE_CONTAINER_NAME, fileName, outputStream);
            }
        }
    }
}