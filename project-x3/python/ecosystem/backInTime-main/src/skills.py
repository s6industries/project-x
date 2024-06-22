import pygame
import random
# Skills = speed, boomerang, shield, Copy - other shield(back to enemy its own attack), ice spike, lightning bolt, mana drain(steal mana)

class Skills:

    def __init__(self, player, screen, scale=None, animated='normal'):

        # player's rect
        self.player = player
        self.animated = animated
        self.screen = screen
        self.triggered = False
        self.level = 1

        self.image = None
        self.animatedImage = []
        self.frame = 0 # for animation
        self.scale = scale
        self.mana = 0
        self.skill_cooldown = 30
        self.range = 0
        self.cooldown = 0

        # check if animated or not
        if self.animated == 'animated':
            self.loadAnimation()
            # skills rect
            self.rect = pygame.Rect(self.player.rect.x, self.player.rect.y, self.scale[0], self.scale[1])
        elif self.animated == 'normal':
            self.loadImage()
            # skills rect
            self.rect = pygame.Rect(self.player.rect.x, self.player.rect.y, self.scale[0], self.scale[1])

    # draw skills
    def draw(self):
        if self.animated == 'animated':
            if (self.frame + 1) >= 21:
                self.frame = 0
            self.screen.blit(self.animatedImage[self.frame//3], self.rect)
            self.frame += 1
        else:
            self.screen.blit(self.image, self.rect)

    # for none animated skills
    def loadImage(self):
        image = f'characters/Skills/{self.player.name}/skill.png'
        image = pygame.image.load(image)
        image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
        self.image = image

    def trigger(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_r] and not(self.triggered) and self.player.mana > 0 and self.player.skill_cooldown >= self.skill_cooldown:
            self.triggered = True
            self.player.skill_cooldown = 0

    # for animated skills
    def loadAnimation(self):
        for count in range(7):
            image = f'characters/Skills/{self.player.name}/skill_{count}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
            self.animatedImage.append(image)

    def Hit(self, enemies, damage, delay=0):
        for enemy in enemies:
            if pygame.sprite.collide_rect(self, enemy):
                if delay <= 0:
                    enemy.attacked = True
                    enemy.life -= (damage + enemy.damage)

    def decreaseMana(self):
        if self.player.mana > 1:
            self.player.mana -= self.mana

class Speed(Skills):

    def __init__(self, player, screen, animated='none'):
        super().__init__(player, screen=screen, animated=animated)
        self.cooldown = 120
        self.speed = 15 # max speed: 15px
        self.mana = 1
        self.skill_cooldown = 35

    def skill(self, enemies=[]):
        if self.triggered and self.cooldown > 0:
            self.player.speed = self.speed
            self.decreaseMana()
            self.cooldown -= 1
        else:
            self.triggered = False
            self.cooldown = 120
            self.player.speed = 7

class Boomerang(Skills):

    def __init__(self, player, screen, scale):
        super().__init__(player, screen, scale)
        self.range = 60
        self.cooldown = self.range
        self.speed = 20
        self.power = 5
        self.mana = 2

    def skill(self, enemies):
        if self.triggered and self.cooldown > 0:
            self.Move()
            self.draw()
            self.Hit(enemies, self.power)
            self.cooldown -= 1
            self.decreaseMana()
        else:
            self.cooldown = self.range
            self.triggered = False
            self.rect.x = (self.player.rect.x + (self.rect.width - self.player.width) / 2) 
            self.rect.y = (self.player.rect.y + (self.rect.width - self.player.height) / 2)

    def Move(self):
        if self.player.up:
            self.rect.y -= self.speed
        elif self.player.down:
            self.rect.y += self.speed
        elif self.player.right:
            self.rect.x += self.speed
        elif self.player.left:
            self.rect.x -= self.speed

class Shield(Skills):

    def __init__(self, player, screen, scale, animated='animated'):
        super().__init__(player, screen, scale, animated=animated)
        self.cooldown = 150
        self.temporarySheild = 600
        self.c_shield = 0
        self.delay = 30 # delay the attack of enemies
        self.damage = 0.5 # damage
        self.mana = 0.8
        self.skill_cooldown = 25

    def skill(self, enemies=[]):
        if self.triggered and self.cooldown > 0:
            self.Use()
            self.rect.x = self.player.rect.x + (self.player.width - self.rect.width) / 2
            self.rect.y = self.player.rect.y + (self.player.height - self.rect.height) / 2
            self.draw()
            self.Hit(enemies, self.damage, self.delay)
            
            if self.delay <= 0:
                self.delay = 30

            self.delay -= 1
            self.cooldown -= 1
            self.decreaseMana()
        else:
            if self.cooldown <= 0:
                self.Remove()
            self.triggered = False
            self.cooldown = 150
            self.player.sheildPower = self.c_shield

    def Use(self):
        if self.cooldown >= 150:
            self.c_shield = self.player.shieldPower
            self.player.shieldPower += self.temporarySheild

    def Remove(self):
        self.player.shieldPower = self.c_shield
        self.c_shield = 0

class Copy(Skills): # random other 3 Skills

    def __init__(self, player, screen, scale, animated='animated'):
        super().__init__(player, screen, scale, animated)
        self.mana = 1.5
        self.randSkills = [
            Shield(player, screen, scale, animated),
            Boomerang(player, screen, scale),
            Speed(player, screen)
        ]
        self.used = None
        self.skill_cooldown = 25

        self.loadImage()
        self.randSkills[0].animated = self.animated
        self.randSkills[1].image = self.image

    def skill(self, enemies):
        if self.triggered:
            self.used = random.choice(self.randSkills)
            self.used.triggered = True
            self.triggered = False
        if self.used != None:
            self.used.skill(enemies)

    def loadImage(self):
        # for boomerang
        image = f'characters/Skills/johny/skill.png'
        image = pygame.image.load(image)
        image = pygame.transform.scale(image, (30, 30))
        self.image = image

        # for animation - jp skill
        for count in range(7):
            image = f'characters/Skills/jp/skill_{count}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
            self.animatedImage.append(image)