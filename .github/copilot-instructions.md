# Repository structure

This repository contains 2 projects.
- API folder contains ASP.NET API project.
- client folder contains React UI project.

# API

We use ASP.NET to make API for our e-commerce website. This project uses .NET 9 and C#.

## Library used:

-   Entity Framework for ORM.
-   MediatR for business layer.
-   Fluent Validation to validate mediatR command/query requests.
-   AutoMapper to map objects.
-   XUnit and FakeItEasy to write unit tests.

## Business layer structure:

-   Project: Ecommerce.Application.
-   Related validators, commands, queries, DTOs are grouped together inside a folder (example: Products folder).
-   Fluent Validation validators are in Validators folder.
-   Commands are in Commands folder.
-   Queries are in Queries folder.
-   DTOs are in DTOs folder.

Each mediatR command/query result must wrapped in Result class in Ecommerce.Application/Core.Result.cs file.

Example of ListProduct query in Ecommerce.Application/Products/Queries folder:

```C#
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class ListProducts
{
    public class Query : IRequest<Result<List<ProductDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var products = await dbContext
                .Products.ProjectTo<ProductDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            return Result<List<ProductDto>>.Success(products);
        }
    }
}

```

# Frontend

We use React to build our e-commerce website. This react app will use ASP.NET API as backend.

## Library used:
- React for building UI.
- Typescript for type safety.
- React Router for routing.
- Tanstack Query for data fetching.
- Mantine for UI components.
- Zod for data validation.

For schema/form validation, we will use Mantine use-form with Zod schema. Mantine use-form is a hook that provides a simple way to manage form state and validation.

For tanstack query, we will make hooks instead of using queryClient directly. A hook may contain multiple queries or mutations. Also we will use customFetch function to make API calls. This function will handle the API calls and return the response data.

Example of a hook that contains multiple queries and mutations:

```typescript

import { useQueryClient, useQuery, useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import { UserInfoResponse } from '../types';
import { customFetch } from '../customFetch';
import { LoginRequest } from '../types';
import { RegisterRequest } from '../types';
import { ChangePasswordRequest } from '../types';

export const useAccount = () => {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { data: currentUser, isLoading: loadingUserInfo } = useQuery({
    queryKey: ['user'],
    queryFn: async () => {
      const data = await customFetch<UserInfoResponse>('/user-info');
      return data;
    },
    enabled: !queryClient.getQueryData(['user']),
  });

  const loginUser = useMutation({
    mutationFn: async (creds: LoginRequest) => {
      await customFetch('/login?useCookies=true', {
        method: 'POST',
        body: JSON.stringify(creds),
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['user'],
      });
    },
  });

  const registerUser = useMutation({
    mutationFn: async (creds: RegisterRequest) => {
      await customFetch('/register', {
        method: 'POST',
        body: JSON.stringify(creds),
      });
    },
  });

  const logoutUser = useMutation({
    mutationFn: async () => {
      await customFetch('/logout', {
        method: 'POST',
      });
    },
    onSuccess: () => {
      queryClient.removeQueries({ queryKey: ['user'] });
      queryClient.removeQueries({ queryKey: ['activities'] });
      navigate('/');
    },
  });

  const changePassword = useMutation({
    mutationFn: async (data: ChangePasswordRequest) => {
      await customFetch('/change-password', {
        method: 'POST',
        body: JSON.stringify(data),
      });
    },
  });

  return {
    loginUser,
    currentUser,
    logoutUser,
    loadingUserInfo,
    registerUser,
    changePassword,
  };
};
```