# Memespector GUI

Memespector GUI is a cross-platform client for [Google Cloud Vision API](https://cloud.google.com/vision/), [Microsoft Cognitive Services Computer Vision](https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/), [Clarifai Computer Vision](https://www.clarifai.com/) and open soruce computer vision models with a graphical user interface.  

Memespector GUI supports Windows, Mac OS and Linux.

![Screenshot](doc/res/memespector-gui-screenshot-mac.png)

# Set-up

1. Download [the latest release](https://github.com/jason-chao/memespector-gui/releases/) for Windows, Mac OS or Linux.

2. You have to get authentication keys from Google Cloud, Microsoft Azure and Clarifai in order to use their APIs.

   1. If you wish to use Google Vision API, you must obtain a credential key file from Google Cloud.  See [the instructions on how to activate Google Cloud Service and obtain an authentication file](doc/GetKeyFromGoogleCloud.md).
   2. If you wish to use Microsoft Cognitive Services Computer Vision, you must obtain a service endpoint and subscription key from Google Cloud.  See [the instructions on how to activate Microsoft Azure and obtain authentication keys](doc/GetKeyFromMicrosoftAuzre.md).
   3. If you wish to use Clarifai Computer Vision, you must obtain an API key from Clarifai. See [the instructions on how to obtain an API key](doc/GetKeyFromClarifai.md).  

# Usage

When you open Memespector-GUI for the first time on Windows or Mac, the operating system may decline to run Memespector-GUI because of tightened security measures.  If this happens, please follow [the instructions to allow Memespector-GUI to run at first launch on Windows and Mac](doc/FirstRun.md).

*Note:  The graphical user interfaces of Memespsector-GUI are nearly identical across the Windows, Mac and Linux versions.  For illustrative purpose, the screenshots of the Windows version are used in this document.  The following steps also apply to the Mac and Linux versions of Memespector-GUI.*

## Change API settings

![Screenshot](doc/res/gui-win-settings.jpg)

### Google Vision

#### Select the credential key file

Check the box for `Google Vision`.  Click `Browse` to the right of the `Credential file` box.

![Screenshot](doc/res/gui-gv-credential-file-field.jpg)

Select the .json file you downloaded from Google Cloud.  See [the detailed instructions on how to obtain a credential file from Google Cloud](doc/GetKeyFromGoogleCloud.md#obtaining-a-credential-key-file).

![Screenshot](doc/res/gui-gv-win-selectcredentialfile.jpg)

#### Select the features to be detected

Check the features that you want Google Vision API to detect in the images.

![Screenshot](doc/res/gui-gv-feature-selection.jpg)

- [Safety](https://cloud.google.com/vision/docs/detecting-safe-search) - adult, violent and racist elements
- [Face](https://cloud.google.com/vision/docs/detecting-faces) - emotional expressions of faces
- [Label](https://cloud.google.com/vision/docs/labels) - generalised labels defined by Google
- [Web](https://cloud.google.com/vision/docs/detecting-web) - web entities (inferred descriptions from similar images on the web), similar images, full/partial matching images, visually similar images and web pages with matching images
- [Text](https://cloud.google.com/vision/docs/ocr) - the text recognised
- [Landmark](https://cloud.google.com/vision/docs/detecting-landmarks) - well-known or prominent sites
- [Logo](https://cloud.google.com/vision/docs/detecting-logos) - logos of popular products

### Microsoft Azure Cognitive Services

#### Paste the Endpoint and Subscription key

Check the box for `Microsoft Azure`.  Paste the Endpoint and Subscription key you obtained from Microsoft Azure into the `Endpoint` and `Subscription key` boxes.    See [the detailed instructions on how to obtain an endpoint and a key from Microsoft Azure](doc/GetKeyFromMicrosoftAzure.md#obtaining-a-service-endpoint-and-a-subscription-key).

![screenshot](doc/res/gui-az-endpoint-key-fields.jpg)

#### Select the features to be detected

Check the features that you want Microsoft Azure Cognitive Services to detect in the images.

![screenshot](doc/res/gui-az-feature-selection.jpg)

- [Adult](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-detecting-adult-content) - explicitly sexual, sexually suggestive and blood/gore
- [Brands](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-brand-detection) - logos of brands in consumer electronics, clothing and more
- [Categories](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/category-taxonomy) - 86-category taxonomy
- [Description](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-describing-images) - a human-readable sentence that describes the image's contents
- [Face](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-detecting-faces) - human faces with age and gender
- [Objects](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-object-detection) - objects or living things with bounding box coordinates
- [Tags](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-tagging-images) - recognisable objects, living beings, scenery and actions

### Clarifai 

Check the box for `Clarifai`.  Paste the API key you obtained from Clarifai into the `API key` box.  See [the detailed instructions on how to obtain an endpoint and a subscription key from Microsoft Azure](doc/GetKeyFromClarifai.md#obtaining-an-api-key).

![screenshot](doc/res/gui-cl-api-key-field.jpg)

#### Select the computer vision model

Pick the model that you want Clarifai to use to analyse the images.

![screenshot](doc/res/gui-cl-model-selection.jpg)

- [General](https://www.clarifai.com/models/image-recognition-ai) - concepts including objects, themes, moods and more
- [Apparel](https://www.clarifai.com/models/clothing-image-recognition) - fashion-related concepts
- [Celebrity](https://www.clarifai.com/models/celebrity-face-recognition) - recognised celebrities 
- [Color](https://www.clarifai.com/models/color-detection) - dominant colours present
- [Food](https://www.clarifai.com/models/ai-food-recognition) - food items
- [Moderation](https://www.clarifai.com/models/ai-image-moderation) - gore, drugs, explicit nudity or suggestive nudity
- [NSFW](https://www.clarifai.com/models/nsfw-model-for-content-detection) - nudity
- [Textures and Patterns](https://www.clarifai.com/models/texture-recognition) - common textures (feathers, woodgrain), unique/fresh concepts (petrified wood, glacial ice) and overarching descriptive concepts (veined, metallic)
- [Travel](https://www.clarifai.com/models/travel-image-recognition) - specific features of residential, hotel and travel-related properties

### Open Source 
 
*Caution: The API that serves open source pre-trained computer vision models is experimental.  It does not offer the same level of performance as the commercial APIs.  The default endpoint in Memespector-GUI is for evaluation purpose only.*

Check the box for `Open Source`.  

![screenshot](doc/res/gui-os-endpoint.jpg)

#### Select the convolutional neural network (only if necessary)

All the models were trained on [ImageNet](https://www.image-net.org/) database.  Just leave the default settings unchanged unless you want to study these models.  The differences between them are in the construction of convolutional neural networks.  If interested in the details, see [here](https://keras.io/api/applications/). 

![screenshot](doc/res/gui-os-model-selection.jpg)

## Add images to Memespector-GUI

### Images on a computer

Click `files on this computer`.  Select the image files.  Click `Open`.  The locations of the image files will be added to the `Image locations` box.

![Screenshot](doc/res/gui-win-individual-files.jpg)

![Screenshot](doc/res/gui-win-individual-file-locations.jpg)

If you want to add all images inside a folder with many sub-folders which also contain images, click `a folder on this computer`.  Select the folder.  The locations of all image files inside the folder and its sub-folders will be added to the `Image locations` box.

![Screenshot](doc/res/gui-win-a-folder.jpg)

![Screenshot](doc/res/gui-win-a-folder-locations.jpg)

### Images on the web

Copy and paste the URLs of the images into the `Image locations` box.  Put one URL per line.

![Screenshot](doc/res/gui-win-web-locations.jpg)

## Edit the names for output files

Memespector GUI generates two output files.  The JSON file stores the structured data values in full generated by the computer vision APIs.  The CSV file contains a version of the data transformed into a tabular format which may be easily opened using general software tools like spreadsheet editors.

Click `Browser` to the right of each field to change the name and location of an output file.

![Screenshot](doc/res/gui-win-outputfiles.jpg)

## Invoke the APIs

Click `Invoke APIs` after you have changed the APIs' settings, added image files or URLs to the `Image locations` box and/or edited the output filenames.

![Screenshot](doc/res/gui-win-invokebutton.jpg)

Memespector-GUI will show the progress of sending the images to the APIs.

![Screenshot](doc/res/gui-win-progress.jpg)

Once all images are processed, a message about the completion will be shown.  Open the output files to see the results.

![Screenshot](doc/res/gui-win-invoke-completion.jpg)

# Credits

Developed by [Jason Chao](https://jasontc.net/).

Special thanks to [Janna Joceli Omena](https://github.com/jannajoceli) for her efforts in idea generation and software testing.

Inspired by the original memespector projects of [bernorieder](https://github.com/bernorieder/memespector) and [amintz](https://github.com/amintz/memespector-python).

# Licence

MIT