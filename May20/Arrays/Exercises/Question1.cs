using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arrays.Exercises
{
    internal class Question1
    {
        public static void Run()
        {
            string input;
            int num_users;

            Console.Write("Enter number of users: ");
            input = Console.ReadLine();
            while (!CheckInput(input))
            {
                Console.Write("Invalid input. Please enter positive integer: ");
                input = Console.ReadLine();

            }
            num_users = Convert.ToInt32(input);

            if (num_users != 0)
            {
                (string, int)[][] total_posts = new (string, int)[num_users][];
                for (int i = 0; i < num_users; i++)
                {
                    total_posts[i] = UserTotalPosts(i + 1);
                }
                Console.WriteLine();
                Console.WriteLine("--Displaying Instagram Posts--");
                PrintAllPosts(total_posts, num_users);
            } else
            {
                Console.WriteLine("No users.Goodbye!");
            }

        }

        
        static (string,int)[] UserTotalPosts(int user_id)
        {
            string input;
            int num_posts;
            Console.WriteLine();
            Console.Write($"User {user_id}: How many Posts? ");
            input = Console.ReadLine();
            while (!CheckInput(input))
            {
                Console.Write("Invalid Input. Please enter positive integer: ");
                input = Console.ReadLine();
            }
            num_posts = Convert.ToInt32(input);
            (string, int)[] user_posts = new (string, int)[num_posts];
            for (int i=0;i<num_posts;i++)
            {
                (string, int) post = GetPost(i + 1);
                user_posts[i] = post;

            }

            return user_posts;

        }

        static (string,int) GetPost(int post_id)
        {
            string caption, likes_ip;
            int num_likes;
            (string, int) post;

            Console.Write($"Enter caption for post {post_id}: ");
            caption = Console.ReadLine();
            Console.Write($"Enter likes: ");
            likes_ip = Console.ReadLine();
            while (!CheckInput(likes_ip))
            {
                Console.Write("Invalid Input. Please enter positive integer: ");
                likes_ip = Console.ReadLine();
            }

            num_likes = Convert.ToInt32(likes_ip);
            post = (caption, num_likes);
            return post;
        }

        static void PrintAllPosts((string, int)[][]total_posts,int num_users)
        {
            for (int i=0;i<num_users;i++)
            {
                (string, int)[] user_posts = total_posts[i];
                PrintUserPosts(user_posts, i + 1);
                Console.WriteLine();
            }
        }

        static void PrintUserPosts((string, int)[] user_posts, int user_id)
        {

            Console.WriteLine($"USER {user_id}: ");
            if (user_posts.Length > 0)
            {
                for (int i = 0; i < user_posts.Length; i++)
                {
                    Console.Write($"Post {i + 1}- ");
                    Console.Write($"Caption: {user_posts[i].Item1}| ");
                    Console.Write($"Likes: {user_posts[i].Item2}");
                    Console.WriteLine();
                }

            } else
            {
                Console.Write("No posts");
            }
        }

        static bool CheckInput(string input)
        {
            int number;
            if (int.TryParse(input, out number))
            {
                if (number >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
