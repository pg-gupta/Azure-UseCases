using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using AdaptiveCards;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using Autofac;

using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;


namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [LuisModel("117d2216-c908-4bac-95c2-fa550627be38", "ecb5f3dbe980438099ca7db16c5b25d9", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class SimpleNoteDialog : LuisDialog<object>,IDialog<object>
    {

        // Store notes in a dictionary that uses the title as a key
        private readonly Dictionary<string, Note> noteByTitle = new Dictionary<string, Note>();

        // Default note title
        public const string DefaultNoteTitle = "default";
        // Name of note title entity
        public const string Entity_Note_Title = "Note.Title";

        List<string> diseases = null;
        /// <summary>
        /// This method overload inspects the result from LUIS to see if a title entity was detected, and finds the note with that title, or the note with the default title, if no title entity was found.
        /// </summary>
        /// <param name="result">The result from LUIS that contains intents and entities that LUIS recognized.</param>
        /// <param name="note">This parameter returns any note that is found in the list of notes that has a matching title.</param>
        /// <returns>true if a note was found, otherwise false</returns>
        public bool TryFindNote(LuisResult result, out Note note)
        {
            note = null;

            string titleToFind;

            EntityRecommendation title;
            if (result.TryFindEntity(Entity_Note_Title, out title))
            {
                titleToFind = title.Entity;
            }
            else
            {
                titleToFind = DefaultNoteTitle;
            }

            return this.noteByTitle.TryGetValue(titleToFind, out note); // TryGetValue returns false if no match is found.
        }

        /// <summary>
        /// This method overload takes a string and finds the note with that title.
        /// </summary>
        /// <param name="noteTitle">A string containing the title of the note to search for.</param>
        /// <param name="note">This parameter returns any note that is found in the list of notes that has a matching title.</param>
        /// <returns>true if a note was found, otherwise false</returns>
        public bool TryFindNote(string noteTitle, out Note note)
        {
            bool foundNote = this.noteByTitle.TryGetValue(noteTitle, out note); // TryGetValue returns false if no match is found.
            return foundNote;
        }


        /// <summary>
        /// Send a generic help message if an intent without an intent handler is detected.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        [LuisIntent("Greeting")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            
            DBConnect dBConnect = new DBConnect();
            string message = "Hello " + string.Join(", ", result.Entities.Select(i => i.Entity));

            message += "How are you?";
            dBConnect.OpenConnection();
            List<string> list=  dBConnect.getData();
            message += string.Join(",", list);
            dBConnect.CloseConnection();

            await context.PostAsync(message);

            context.Wait(MessageReceived);

        }




        [LuisIntent("Sick")]
        public async Task Sick(IDialogContext context, LuisResult result){

            string message = "Do not worry! I can help you.\n Tell me how are you feeling or tell me your symptoms?";
            var symptoms = result.Entities.Select(x => x.Entity);
            await context.PostAsync(message);
            context.Wait(MessageReceived);                      
        }

        [LuisIntent("Symptoms")]
        public async Task Symptoms(IDialogContext context, LuisResult result)
        {
            string message = "You might be suffering from ";

            var symptoms = result.Entities.Select(x => x.Entity).ToList();
            DBConnect dBConnect = new DBConnect();
            dBConnect.OpenConnection();
            diseases = dBConnect.getDiseases(symptoms);
            message += string.Join(",", diseases);
            dBConnect.CloseConnection();
            await  context.PostAsync(message);
            context.Wait(MessageReceived);
          

        }

        [LuisIntent("Medicine")]
        public async Task Medicine(IDialogContext context, LuisResult result)
        {
            string message = "";
            if (diseases == null)
                message = "Please tell me your symptoms first";
            else
            {
                message = "You may take following medicines ";
                DBConnect dBConnect = new DBConnect();
                dBConnect.OpenConnection();
                List<string> list = dBConnect.getMedicines(diseases);
                message += string.Join(",", list);
                dBConnect.CloseConnection();
            }

            await context.PostAsync(message);
            context.Wait(MessageReceived);


        }
        /// <summary>
        /// Handle the Note.Delete intent. If a title isn't detected in the LUIS result, prompt the user for a title.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        /// <returns></returns>
        [LuisIntent("Note.Delete")]
        public async Task DeleteNote(IDialogContext context, LuisResult result)
        {
            Note note;
            if (TryFindNote(result, out note))
            {
                this.noteByTitle.Remove(note.Title);
                await context.PostAsync($"Note {note.Title} deleted");
            }
            else
            {                             
                // Prompt the user for a note title
                PromptDialog.Text(context, After_DeleteTitlePrompt, "What is the title of the note you want to delete?");                         
            }

        }

        private async Task After_DeleteTitlePrompt(IDialogContext context, IAwaitable<string> result)
        {
            Note note;
            string titleToDelete = await result;
            bool foundNote = this.noteByTitle.TryGetValue(titleToDelete, out note);

            if (foundNote)
            {
                this.noteByTitle.Remove(note.Title);
                await context.PostAsync($"Note {note.Title} deleted");
            }
            else
            {
                await context.PostAsync($"Did not find note named {titleToDelete}.");
            }

            context.Wait(MessageReceived);
        }

        /// <summary>
        /// Handles the Note.ReadAloud intent by displaying a note or notes. 
        /// If a title of an existing note is found in the LuisResult, that note is displayed. 
        /// If no title is detected in the LuisResult, all of the notes are displayed.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">LUIS result.</param>
        /// <returns></returns>
        [LuisIntent("Note.ReadAloud")]
        public async Task FindNote(IDialogContext context, LuisResult result)
        {
            Note note;
            if (TryFindNote(result, out note))
            {
                await context.PostAsync($"**{note.Title}**: {note.Text}.");
            }
            else
            {
                // Print out all the notes if no specific note name was detected
                string NoteList = "Here's the list of all notes: \n\n";
                foreach (KeyValuePair<string, Note> entry in noteByTitle)
                {
                    Note noteInList = entry.Value;
                    NoteList += $"**{noteInList.Title}**: {noteInList.Text}.\n\n";
                }
                await context.PostAsync(NoteList);
            }

            context.Wait(MessageReceived);
        }

        private Note noteToCreate;
        private string currentTitle;

        /// <summary>
        /// Handles the Note.Create intent. Prompts the user for the note title if the title isn't detected in the LuisResult.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">LUIS result.</param>
        /// <returns></returns>
        [LuisIntent("Note.Create")]
        public Task CreateNote(IDialogContext context, LuisResult result)
        {
            EntityRecommendation title;
            if (!result.TryFindEntity(Entity_Note_Title, out title))
            {
                // Prompt the user for a note title
                PromptDialog.Text(context, After_TitlePrompt, "What is the title of the note you want to create?");
            }
            else
            {
                var note = new Note() { Title = title.Entity };
                noteToCreate = this.noteByTitle[note.Title] = note;

                // Prompt the user for what they want to say in the note           
                PromptDialog.Text(context, After_TextPrompt, "What do you want to say in your note?");
            }

            return Task.CompletedTask;
        }

        private async Task After_TitlePrompt(IDialogContext context, IAwaitable<string> result)
        {
            EntityRecommendation title;
            // Set the title (used for creation, deletion, and reading)
            currentTitle = await result;
            if (currentTitle != null)
            {
                title = new EntityRecommendation(type: Entity_Note_Title) { Entity = currentTitle };
            }
            else
            {
                // Use the default note title
                title = new EntityRecommendation(type: Entity_Note_Title) { Entity = DefaultNoteTitle };
            }

            // Create a new note object 
            var note = new Note() { Title = title.Entity };
            // Add the new note to the list of notes and also save it in order to add text to it later
            noteToCreate = this.noteByTitle[note.Title] = note;

            // Prompt the user for what they want to say in the note           
            PromptDialog.Text(context, After_TextPrompt, "What do you want to say in your note?");

        }

        private async Task After_TextPrompt(IDialogContext context, IAwaitable<string> result)
        {
            // Set the text of the note
            noteToCreate.Text = await result;
            
            await context.PostAsync($"Created note **{this.noteToCreate.Title}** that says \"{this.noteToCreate.Text}\".");
            
            context.Wait(MessageReceived);
        }


        public SimpleNoteDialog()
        {

        }

        public SimpleNoteDialog(ILuisService service)
            : base(service)
        {
            
        }

        [Serializable]
        public sealed class Note : IEquatable<Note>
        {

            public string Title { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return $"[{this.Title} : {this.Text}]";
            }

            public bool Equals(Note other)
            {
                return other != null
                    && this.Text == other.Text
                    && this.Title == other.Title;
            }

            public override bool Equals(object other)
            {
                return Equals(other as Note);
            }

            public override int GetHashCode()
            {
                return this.Title.GetHashCode();
            }
        }
    }


}