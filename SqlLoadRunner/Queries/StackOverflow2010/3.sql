	  select p.CreationDate, p.Body
  from [StackOverflow2010].dbo.Users u join [StackOverflow2010].[dbo].[Posts] p
	on p.OwnerUserId = u.Id	
  where u.Id = 1
  order by p.CreationDate
