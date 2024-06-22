import pygame
import random
from . import weapons
import random
import math
from . import timer

# ENEMY BOSS FIGHT

class Boss(pygame.sprite.Sprite):
    
    def __init__(self, x, y, width, height, name, screen):
        self.width = width
        self.height = height
        self.name = name
        self.screen = screen
        self.rect = pygame.Rect(x, y, self.width, self.height)

        self.defaultLife = None
        self.life = 0
        self.mana = 500
        self.damage = 0
        self.walk = 0
        self.speed = 7

        self.weapon = []
        self.skills = ['weapon'] # 0 means no skills to do
        self.darkBalls = []

        self.attack = False
        self.follow = True # follow the toFocus if true
        self.toFocus = None # player or other objects
        self.FocusList = []
        self.change = False # changing what to focus or to attack
        self.isMoving = False
        self.show = True
        self.disableSkill = False
        self.hold = False
        self.overflow = False

        self.init_boss()

    def draw(self):
        self.isMoving = False
        if (self.walk + 1) >= 9:
            self.walk = 0

        self.Facing() # handle where the boss facing
        self.Skills() # handling the skills of the boss
        self.Move() # make boss move

        if not self.hold and self.overflow:
            self.avoidEdge()
        
        self.weapon[0].weapon([self.toFocus]) # wapon of the boss
        
        if self.show:
            if self.isMoving:
                self.screen.blit(self.c_facing[self.walk//3], self.rect)
            else:
                self.c_facing = self.c_down
                self.screen.blit(self.c_facing[0], self.rect)
            self.walk += 1

            self.barLife()

    def barLife(self):
        pygame.draw.rect(self.screen, (207, 208, 255), (self.rect.x, self.rect.y - (self.height / 2) + 10, self.width, 5))
        pygame.draw.rect(self.screen, (252, 78, 15), (self.rect.x, self.rect.y - (self.height / 2) + 10, (self.life / self.defaultLife) * self.width, 5))

    def Facing(self):
        if self.direction == 'left':
            self.c_facing = self.c_left
        elif self.direction == 'right':
            self.c_facing = self.c_right
        elif self.direction == 'up':
            self.c_facing = self.c_up
        elif self.direction == 'down':
            self.c_facing = self.c_down

    def Move(self): # this function for following toFocus
        if self.follow and self.toFocus != None:
            if self.toFocus.rect.x > self.rect.x:
                self.rect.x += self.speed
                self.direction = 'right'
                self.isMoving = True
            if self.toFocus.rect.x < self.rect.x:
                self.rect.x -= self.speed
                self.direction = 'left'
                self.isMoving = True
            if self.toFocus.rect.y > self.rect.y:
                self.rect.y += self.speed
                self.direction = 'down'
                self.isMoving = True
            if self.toFocus.rect.y < self.rect.y:
                self.rect.y -= self.speed
                self.direction = 'up'
                self.isMoving = True

    def loadImages(self):
        sides = ['D_Walk_', 'U_Walk_', 'S_Walk_']

        for side in sides:
            for i in range(3):
                image = f'characters/test/{side}{i}.png' # replace f'character/{self.name}/{side}{i}.png'
                image = pygame.image.load(image)
                image = pygame.transform.scale(image, (self.width, self.height))
                if side == 'D_Walk_':
                    self.c_down.append(image)
                elif side == 'U_Walk_':
                    self.c_up.append(image)
                elif side == 'S_Walk_':
                    self.c_left.append(image)

        for i in self.c_left:
            image = pygame.transform.flip(i, True, False)
            self.c_right.append(image)

    def init_boss(self):
        # Boss images loaded, facing and direction
        self.c_left = []
        self.c_right = []
        self.c_up = []
        self.c_down = []
        self.c_hit = None
        self.c_facing = self.c_down
        self.direction = 'right'
        self.loadImages()

    def change_focus(self):
        if self.change:
            self.toFocus = random.choice(self.FocusList)
            self.change = False

    # at the end of the battle - if player win
    def addLevel(self, player):
        if player.level <= 3: # level 1 to 3 only
            player.level += 1
            self.player.myWeapons += self.weapon # give to the player the weapons

    def move_x(self, direction):
        self.rect.x += direction

    def move_y(self, direction):
        self.rect.y += direction

    def Skills(self):
        if random.choice(range(50)) == 0 and not self.disableSkill:
            skill = random.choice(self.skills)
            if skill == self.skills[0]:
                posx = self.toFocus.rect.x
                posy = self.toFocus.rect.y
                self.weapon[0].BossTraget((posx, posy))

    def Reset(self):
        self.rect.x = 100
        self.rect.y = 100
        self.life = self.defaultLife
        self.mana = 500
        self.dash = False
        self.hold = False
        self.darkBalls = []

    def avoidEdge(self):
        if self.rect.x <= self.width:
            self.rect.x += self.speed
            self.direction = 'right'
            self.isMoving = True
        elif self.rect.x >= 700-self.width:
            self.rect.x -= self.speed
            self.direction = 'left'
            self.isMoving = True
        elif self.rect.y <= self.height:
            self.rect.y += self.speed
            self.direction = 'down'
            self.isMoving = True
        elif self.rect.y >= 500-self.height:
            self.rect.y -= self.speed
            self.direction = 'up'
            self.isMoving = True
        else:
            self.isMoving = False

class Ethan(Boss):

    def __init__(self, x, y, width, height, name='ethan', screen=None):
        super().__init__(x, y, width, height, name, screen)
        self.defaultLife = 500
        self.life = self.defaultLife
        self.overflow = True

        self.weapon = [
            weapons.Mjolnir(self, self.screen, (30,30))
        ]
        self.potion = [
            weapons.Potions(self, self.screen, (25, 25), 'speed')
        ]

        self.dashSpeed = 20
        self.dash = False
        self.canDash = False
        self.timer = timer.Timer(30)
        self.hold = False

    def Move(self):
        if not random.choice(range(0, 300)):
            self.dash = True

        if self.dash:
            if self.canDash:
                self.isMoving = True
                dir_x = self.toFocus.rect.x - self.rect.x
                dir_y = self.toFocus.rect.y - self.rect.y

                distance = math.sqrt(dir_x ** 2 + dir_y ** 2)
                
                if distance:
                    dir_x /= distance
                    dir_y /= distance
                self.rect.x += dir_x * self.dashSpeed
                self.rect.y += dir_y * self.dashSpeed

                if dir_x > 0:
                    self.direction = 'right'
                else:
                    self.direction = 'left'

                if distance < self.speed:
                    self.dash = False
                    self.toFocus.can_move = False
                    self.hold = True
                    self.disableSkill = True
                    self.canDash = False

        if self.hold:
            if self.timer.coolDown(3):
                self.toFocus.can_move = True
                self.disableSkill = False
                self.hold = False

        self.dashCooldown()

    def dashCooldown(self):
        if not self.canDash:
            if self.timer.coolDown(30):
                self.canDash = True

class Aeron(Boss):

    def __init__(self, x, y, width, height, name='aeron', screen=None):
        super().__init__(x, y, width, height, name, screen)
        self.defaultLife = 80
        self.life = self.defaultLife
        self.speed = 8
        self.damage = 30
        self.timer = timer.Timer(30)

        self.weapon = [
            weapons.Trident(self, self.screen, (50,50)), # trident weapon
        ]
        self.skills = ['trident', 'darkball']

        self.darkBalls = []
        self.rain_darkball = False
        self.wait = 10
        self.count = 0

        self.potion = [
            weapons.Potions(self, self.screen, (25, 25), 'weaponize') # weaponized potion
        ]

    def Move(self):
        point_a, point_b = 50, 650
        if not self.rain_darkball:
            if self.direction == 'right':
                self.rect.x += self.speed
                self.isMoving = True
                if self.rect.x >= point_b:
                    self.direction = 'left'
            elif self.direction == 'left':
                self.rect.x -= self.speed
                self.isMoving = True
                if self.rect.x <= point_a:
                    self.direction = 'right'

    def Skills(self):
        if random.choice(range(50)) == 0 and not self.disableSkill and not self.rain_darkball:
            skill = random.choice(self.skills)
            if skill == self.skills[0]: # throw a trident
                posx = self.toFocus.rect.x
                posy = self.toFocus.rect.y
                self.weapon[0].BossTraget((posx, posy)) # throw a weapon [mjolnir, trident, boomerang]
            elif skill == self.skills[1]:
                self.rain_darkball = True
                [self.darkBalls.append(weapons.DarkBall(self, self.screen, (25,25))) for _ in range(20)]

        self.Rain()

    def Rain(self):
        if self.rain_darkball:
            for darkball in self.darkBalls:
                darkball.weapon([self.toFocus])
                if darkball.remove:
                    self.darkBalls.remove(darkball)
            if len(self.darkBalls) < 1:
                self.rain_darkball = False

class Christian(Boss):

    def __init__(self, x, y, width, height, name='christian', screen=None):
        super().__init__(x, y, width, height, name, screen)
        self.defaultLife = 1200
        self.life = self.defaultLife

    def loadImages(self):
        for side in range(4):
            image = f'characters/{self.name}/side_{side+1}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.width, self.height))
            self.c_right.append(image)

        for image in self.c_right: # flip image
            image = pygame.transform.flip(image, True, False)
            self.c_left.append(image)