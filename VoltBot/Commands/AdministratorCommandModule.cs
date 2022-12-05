﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VoltBot.Commands
{
    [RequireUserPermissions(Permissions.Administrator)]
    internal class AdministratorCommandModule : BaseCommandModule
    {
        [Command("redirect")]
        [Aliases("rd")]
        [Description("Переслать сообщение в другой канал и удалить его с предыдущего")]
        public async Task Redirect(CommandContext ctx,
            [Description("Канал, куда необходимо переслать сообщение")] DiscordChannel targetChannel,
            [Description("Причина (необязательно)"), RemainingText] string reason = null)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
              .WithTitle(ctx.Member.DisplayName)
              .WithColor(EmbedConstants.ErrorColor);

            if (ctx.Message.Reference == null)
            {
                discordEmbed.WithDescription("Вы не указали сообщение, которое необходимо переслать");
                await ctx.RespondAsync(discordEmbed);
            }
            else if (targetChannel == null)
            {
                discordEmbed.WithDescription("Вы не указали канал, куда необходимо переслать сообщение");
                await ctx.RespondAsync(discordEmbed);
            }
            else
            {
                DiscordMessage redirectMessage = await ctx.Channel.GetMessageAsync(ctx.Message.Reference.Message.Id);

                discordEmbed.WithColor(EmbedConstants.SuccessColor)
                    .WithFooter($"Guild: {redirectMessage.Channel.Guild.Name}, Channel: {redirectMessage.Channel.Name}, Time: {redirectMessage.CreationTimestamp}")
                    .WithDescription(redirectMessage.Content)
                    .WithTitle(null);

                if (!string.IsNullOrEmpty(reason))
                {
                    discordEmbed.AddField("Причина перенаправления", reason);
                }

                if (redirectMessage.Author != null)
                {
                    discordEmbed.WithAuthor(
                        name: redirectMessage.Author.Username,
                        iconUrl: redirectMessage.Author.AvatarUrl);
                }

                DiscordMessageBuilder newMessage = new DiscordMessageBuilder();

                newMessage.AddEmbed(discordEmbed);

                if (redirectMessage.Embeds?.Count > 0)
                {
                    newMessage.AddEmbeds(redirectMessage.Embeds);
                }

                if (redirectMessage.Attachments?.Count > 0)
                {
                    newMessage.AddEmbeds(redirectMessage.Attachments
                        .Select(x =>
                        {
                            DiscordEmbedBuilder attacmentEmbed = new DiscordEmbedBuilder().WithColor(EmbedConstants.SuccessColor);
                            if (x.MediaType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                            {
                                attacmentEmbed.WithImageUrl(x.Url);
                            }
                            else
                            {
                                attacmentEmbed.WithUrl(x.Url);
                            }
                            return attacmentEmbed.Build();
                        }));
                }

                await targetChannel.SendMessageAsync(newMessage);
                await ctx.Message.DeleteAsync();
            }
        }
    }
}