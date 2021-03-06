using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Markdig;
using Markdig.Parsers;
using Markdig.Extensions.AutoLinks;

namespace WDHAN
{
    public class Post : Page
    {
        public Post()
        {
            
        }
        public static Post getDefinedPost(Post post)
        {
            return (Post) getDefinedPage(post);

            /*
            post.name = Path.GetFileName(post.path);
            post.dir = Path.GetDirectoryName(post.path);

            try
            {
                post.tags = JsonConvert.DeserializeObject<List<string>>(post.frontmatter.GetValue("tags").ToString());
            }
            catch(NullReferenceException)
            {

            }

            try
            {
                post.date = JsonConvert.DeserializeObject<DateTime>(post.frontmatter.GetValue("date").ToString());
            }
            catch(NullReferenceException)
            {

            }

            try
            {
                post.excerpt = getExcerpt(post);
            }
            catch(NullReferenceException)
            {

            }

            return post;
            */
        }
        public static List<Post> getPosts(string collection)
        {
            var siteConfig = GlobalConfiguration.getConfiguration();
            List<Post> postList = new List<Post>();
            var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
            builder.BlockParsers.TryRemove<IndentedCodeBlockParser>();
            var pipeline = builder.Build();
            builder.Extensions.Remove(pipeline.Extensions.Find<AutoLinkExtension>());


            foreach(var post in Directory.GetFiles(siteConfig.collections_dir + "/_" + collection))
            {
                if(GlobalConfiguration.isMarkdown(Path.GetExtension(post).Substring(1)))
                {
                    if(!Path.GetFileNameWithoutExtension(post).Equals("index", StringComparison.OrdinalIgnoreCase))
                    {
                        postList.Add(getDefinedPost(new Post() { frontmatter = parseFrontMatter(post),
                        content = Markdown.ToHtml(WDHANFile.parseRaw(post), pipeline),
                        path = post }));
                    }
                }
            }


            foreach(var post in postList)
            {
                Console.WriteLine(post);
            }
            
            try
            {
                postList.Sort((y, x) => x.frontmatter["date"].ToString().CompareTo(y.frontmatter["date"].ToString()));
                postList.Sort((y, x) => x.title.CompareTo(y.title));
            }
            catch(NullReferenceException)
            {
                postList.Sort((y, x) => x.title.CompareTo(y.title));
            }

            return postList;
        }
        public static void generateEntries()
        {
            var siteConfig = GlobalConfiguration.getConfiguration();
            foreach(var collection in siteConfig.collections)
            {
                Collection collectionPosts = new Collection();
                collectionPosts.entries = getPosts(collection);
                string collectionSerialized = JsonConvert.SerializeObject(collectionPosts, Formatting.Indented);
                Directory.CreateDirectory(siteConfig.source + "/temp/_" + collection);
                using (FileStream fs = File.Create(siteConfig.source + "/temp/_" + collection + "/_entries.json"))
                {
                    fs.Write(Encoding.UTF8.GetBytes(collectionSerialized), 0, Encoding.UTF8.GetBytes(collectionSerialized).Length);
                }
            }
        }
    }
}