// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CRIS.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Transcription
    /// </summary>
    public partial class Transcription : Entity
    {
        /// <summary>
        /// Initializes a new instance of the Transcription class.
        /// </summary>
        public Transcription()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Transcription class.
        /// </summary>
        /// <param name="modelsProperty">A list of models used for the
        /// transcription.
        /// The list may contain an acoustic model, a language model or both.
        /// IF only one model is given, the base model will be used for the
        /// other part</param>
        /// <param name="id">The identifier of this entity</param>
        /// <param name="recordingsUrl">The location where to download the
        /// input data from</param>
        /// <param name="locale">The locale of the contained data</param>
        /// <param name="createdDateTime">The time-stamp when the object was
        /// created</param>
        /// <param name="lastActionDateTime">The time-stamp when the current
        /// status was entered</param>
        /// <param name="status">The status of the object. Possible values
        /// include: 'NotStarted', 'Running', 'Succeeded', 'Failed'</param>
        /// <param name="name">The name of the object</param>
        /// <param name="resultsUrls">The results Urls for the
        /// transcription</param>
        /// <param name="statusMessage">The failure reason for the
        /// transcription</param>
        /// <param name="description">The description of the object</param>
        /// <param name="properties">The custom properties of this
        /// entity</param>
        public Transcription(IList<Model> modelsProperty, System.Guid id, string recordingsUrl, string locale, System.DateTime createdDateTime, System.DateTime lastActionDateTime, string status, string name, IDictionary<string, string> resultsUrls = default(IDictionary<string, string>), string statusMessage = default(string), string description = default(string), IDictionary<string, string> properties = default(IDictionary<string, string>))
        {
            ModelsProperty = modelsProperty;
            Id = id;
            RecordingsUrl = recordingsUrl;
            Locale = locale;
            ResultsUrls = resultsUrls;
            StatusMessage = statusMessage;
            CreatedDateTime = createdDateTime;
            LastActionDateTime = lastActionDateTime;
            Status = status;
            Name = name;
            Description = description;
            Properties = properties;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets a list of models used for the transcription.
        /// The list may contain an acoustic model, a language model or both.
        /// IF only one model is given, the base model will be used for the
        /// other part
        /// </summary>
        [JsonProperty(PropertyName = "models")]
        public IList<Model> ModelsProperty { get; set; }

        /// <summary>
        /// Gets or sets the location where to download the input data from
        /// </summary>
        [JsonProperty(PropertyName = "recordingsUrl")]
        public string RecordingsUrl { get; set; }

        /// <summary>
        /// Gets or sets the results Urls for the transcription
        /// </summary>
        [JsonProperty(PropertyName = "resultsUrls")]
        public IDictionary<string, string> ResultsUrls { get; set; }

        /// <summary>
        /// Gets or sets the failure reason for the transcription
        /// </summary>
        [JsonProperty(PropertyName = "statusMessage")]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (ModelsProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ModelsProperty");
            }
            if (RecordingsUrl == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "RecordingsUrl");
            }
            if (Locale == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Locale");
            }
            if (Status == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Status");
            }
            if (Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Name");
            }
            if (ModelsProperty != null)
            {
                foreach (var element in ModelsProperty)
                {
                    if (element != null)
                    {
                        element.Validate();
                    }
                }
            }
        }
    }
}
